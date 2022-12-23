# 'eng' directory
## Directory Structure
### 'actions'
- This directory is beeing used by GitHub Actions only. This directory contains:
  - 'templates' : GitHub Actions job and step templates used by the workflows.

#### Notable Files
[setup-msbuild.yml](templates/steps/setup-msbuild): step template setting up the correct VS version.

### 'scripts'
- This directory contains:
  -  Scripts needed to build and publish the extension