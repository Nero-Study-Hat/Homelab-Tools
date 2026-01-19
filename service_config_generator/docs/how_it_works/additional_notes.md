# Additional Notes

### Tool usage approach explanation.

Before the role is run every time, the target ansible dir will have the files and directories to be written to cleared of all contents which will all be generated anew.

Deleting existing roles and host_vars files on entry is required because the way the host_var files are generated requires each role to be added and there is no clean way to cut content per service out selectively built yet and it would take a decent amount of time for little benefit as that separation.

Also roles need to generated in a specific order, that order being dictated by the order.txt file at the top level config data directory.

### `tasks.yaml` explanation.

The `tasks.yaml` file can be used instead of tasks dir with manually added tasks plus automated boiler plate when the boiler plate is different. In this case make sure to still specify files and templates in the files and templates blocks of the config.json because while the paths don't matter and tasks are not generated from config this way, this part of the config does inform the tool to copy those files to the target ansible role dir.

Not auto copying all files from those directories over files may be in those locations incorrectly and this effectively asks the user to make sure the right contents are in them.

### `tasks` directory explanation.
main.yaml and post_deploy.yaml, neither required, neither is dir required


### `defaults.yml` and ansible_vars explanation.

The `defaults.yml` file can be used to provide default vars for a role but is not required and generally not used because
- unnessary: does not initialize vars, only sets fallback values which I don't need as all values are set via host_vars
    - what few vars this kind of makes sense for don't justify all the extra files just to avoid another var in host_vars
- problem-maker: when the default role_var has the same as a host_var and one is assigned to another than a referential-loop is made
    - as part of this all vars used in role can't have same name as a host_var var name considering host I the include_role task module and host_vars
- bind all deployments to one value: host_vars is preferred with vars prepared per instance deployment this way there are no conflicts with multi-instance per host deployment


### `host_vars` directory explanation.

This directory can not exist. File names should reflect intended host per respective file and each file name (host name) present should be listed in hosts block in the `config.json`.


### `compose.yaml` explanation.

not required


### On-call service roles explanations.

edgeshark

- reserve their IP addresses even if they are not in use all the time
- create an on-call data dir with this structure
    - top level
        - service_name
            - day_version
            - dusk_version
            - night_version