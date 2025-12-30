// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Text;

namespace RemoteDebuggerLauncher.Infrastructure
{
   /// <summary>
   /// Provides extension methods for <see cref="StringBuilder"/> to deal with ANSI/VT escape sequences.
   /// </summary>
   internal static class StringBuilderExtensions
   {
      private enum EscState
      {
         Normal,
         Esc,          // Saw ESC
         Csi,          // ESC '[' ... until final @-~
         Osc,          // ESC ']' ... terminated by BEL (0x07) or ST (ESC '\')
         DcsLike,      // ESC P/X/^/_ ... terminated by ST (ESC '\')
         St_Esc        // We saw ESC inside OSC/DCS-like; expecting '\' for ST
      }

      /// <summary>
      /// Removes ANSI/VT escape sequences from the content.
      /// </summary>
      /// <param name="sb">The <see cref="StringBuilder"/> to strip ANSI/VT sequences from.</param>
      /// <returns>A <see cref="string"/> without ANSI/VT sequences.</returns>
      public static string ToStringStripAnsi(this StringBuilder sb)
      {
         var output = new StringBuilder(sb.Length);
         var state = EscState.Normal;

         for (int i = 0; i < sb.Length; i++)
         {
            char ch = sb[i];
            switch (state)
            {
               case EscState.Normal:
                  if (ch == '\x1B') // ESC
                  {
                     state = EscState.Esc;
                  }
                  else
                  {
                     _ = output.Append(ch);
                  }
                  break;

               case EscState.Esc:
                  // After ESC:
                  // '[' -> CSI
                  // ']' -> OSC
                  // 'P','X','^','_' -> DCS/SOS/PM/APC
                  // otherwise: single ESC + one char -> drop both
                  if (ch == '[')
                  {
                     state = EscState.Csi;
                  }
                  else if (ch == ']')
                  {
                     state = EscState.Osc;
                  }
                  else if (ch == 'P' || ch == 'X' || ch == '^' || ch == '_')
                  {
                     state = EscState.DcsLike;
                  }
                  else
                  {
                     state = EscState.Normal; // single ESC + one char => skip
                  }

                  break;

               case EscState.Csi:
                  // Params (0-?), intermediates (space-/), then final (@-~)
                  if (IsParam(ch) || IsIntermed(ch))
                  {
                     // consume
                  }
                  else if (IsCsiFinal(ch))
                  {
                     state = EscState.Normal; // finished CSI, drop
                  }
                  else
                  {
                     // malformed, bail out
                     state = EscState.Normal;
                  }
                  break;

               case EscState.Osc:
                  // OSC: content until BEL or ST (ESC '\')
                  if (ch == '\x07') // BEL
                  {
                     state = EscState.Normal;
                  }
                  else if (ch == '\x1B')
                  {
                     state = EscState.St_Esc; // might be ST
                  }
                  else
                  {
                     // consume content
                  }
                  break;

               case EscState.DcsLike:
                  // DCS/SOS/PM/APC: content until ST (ESC '\')
                  if (ch == '\x1B')
                  {
                     state = EscState.St_Esc;
                  }
                  else
                  {
                     // consume content
                  }
                  break;

               case EscState.St_Esc:
                  // Expect '\' to terminate (ST)
                  if (ch == '\\')
                  {
                     state = EscState.Normal;
                  }
                  else
                  {
                     // Not ST; go back to content of a string-terminated seq.
                     // If you want exact origin (OSC vs DCS-like), track a return state.
                     state = EscState.Osc; // safe fallback (both consume until ESC '\')
                  }
                  break;
            }
         }

         return output.ToString();
      }

      private static bool IsCsiFinal(char c) => c >= '@' && c <= '~'; // 0x40–0x7E
      private static bool IsIntermed(char c) => c >= ' ' && c <= '/'; // 0x20–0x2F
      private static bool IsParam(char c) => c >= '0' && c <= '?';    // 0x30–0x3F
   }
}
