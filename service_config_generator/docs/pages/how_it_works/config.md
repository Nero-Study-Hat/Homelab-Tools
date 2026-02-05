# Config Spec

### ProjectName: `string`

The name of the container in case multiple instances of the same container image are used.

For many services they will involve a service stack where each container in the compose get a hard-coded name and this var is never used in the compose.

example: `commafeed`

### NetworkSetup: `string`

The type of network used for the compose file. Manages what containers are reachable in the docker network the container(s) for this role are deployed in.

Options include:
- `t3_only`: putting the container(s) on a docker network with the `main traefik container` for that host
- `ts_only`: putting the container(s) on a docker network with the `main tailscale container` for that host
- `shared`: putting the container(s) on a docker network with `both` the `main traefik container` and `main tailscale container` for that host
- `none`: generate and do nothing

### UserGateDetails: `Dictionary<string, Dictionary<string, List<string>>>`

For context a service role could be used on multiple hosts concurrently, an example being the Network role which is deployed on all hosts.
A service can contain multiple container instances which require a block in the UserGateVar.

The format here is
- key (string): host name
- value (dictionary):
    - key: container instance sub-domain preceding host.wiresndreams.dev
    - value (list of strings): tailscale host ip address # name of each permitted host

note: can be empty

### DirectoriesToCreate: `string[]`

Directories to create on remote. Any directories needed by templates and files should be named here.

note: can be empty

### TemplatesRemoteRelativePaths: `Dictionary<string, string>`

- key: name of template file
- value: target path of template file on remote

warning: no additional files should be in the `config-data-dir/templates-dir` beyond the templates themselves

note: can be empty *(if templates are present in templates config data directory then this should not be empty)

### FilesRemoteRelativePaths: `Dictionary<string, string>`

- key: name of file
- value: target path of file on remote

warning: no additional files should be in the `config-data-dir/files-dir` beyond the files themselves

note: can be empty *(if files are present in files config data directory then this should not be empty)

### UserGateHost: `string`

Which host is the UserGateHost to know which file to edit with those details.

### Hosts: `string[]`

Lists all hosts the service role is used on. For the purpose of editing each host vars file.

Notes:
- Exists separately from UserGateDetails because there could be a case where a service is deployed without intention to be accessed and this provides a simpler variable to use and access.
- can be empty *(if files are in host_vars config data directory then this should not be empty)