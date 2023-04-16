# SSH Key Pair
This section illustrates how to generate a SSH key pair that can be used to authenicate against the target device.

## Supported Key Types
*SSH.NET* supports the following private key formats:
- RSA in OpenSSL PEM and ssh.com format
- DSA in OpenSSL PEM and ssh.com format
- ECDSA 256/384/521 in OpenSSL PEM and OpenSSH key format
- ED25519 in OpenSSH key format

## Generate key Pair on Windows 10 or newer
Use the follwing steps to generate an RSA key pair supported by this extension.
Open a Command Prompt (cmd.exe) and execute the following command:

```
ssh-keygen -b 2048 -t rsa -f id_rsa -q -N "" -m pem
```
This will create a 2048bit RSA keypair with no passphrase stored in the file *id_rda*

By default, the generated SSH keys get stored in %USERPROFILE%\\.ssh
- id_rsa holds the key pair (public and private)
- id_rsa.pub holds the public key

## Add SSH key to authorized_keys
Use the following steps if you want to manually add the generated keys to the 'authorized_keys' file on the target system. Open a Window Commmand Prompt and execute the following command:

```
type %USERPROFILE%\.ssh\id_rsa.pub | ssh USER@HOST "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys"
```

## Add the SSH server fingerprints to known_hosts
Use the following command template to add the SSH server fingerprints to the 'known_hosts' file on the local development PC. Open a Window Commmand Prompt and execute the following command:

```
ssh-keyscan HOST >> %USERPROFILE%\.ssh\known_hosts
```

## References
This extension relies on [SSH.NET](https://github.com/sshnet/SSH.NET) at version 2020.0.2
