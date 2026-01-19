## Usage
`dotnet build` - see if every looks good
`dotnet run`
- will ask for data dir, this is where config and such for what you generate is
- config has field for ansible dir, this is where changes will be made
    - test ansible dir is: `/service_config_generator/tests/TestData/VarHandler/TestAnsibleDir`

---

## Why this tool exists.

### Centralize

Before without this tool when I added a service to my infra I needed to

update file contents for files in multiple different parts of the project in sync
- /ansible/inventory/host_vars/: content `in ansible inventory dir`
    - service_host_file
    - user_gate_host_file

- /ansible/roles/role_name/: content `in ansible role dir`
    - defaults/main.yaml
    - tasks/main.yaml
    - templates/ *all files*
    - files/ *all files*

- /ansible/playbooks/service_host_deploy_file: content `in ansible playbook dir`

The killer being the in sync part as every time I was jumping back and forth across the project to make sure the same variable names were being used and other details were synced up it all would play nice together.

Now with this tool I can do all service specific work in the config data directory for that service and not need to multiple places in the project.

- /ansible/inventory/host_vars/
    - service_host_file: handled with mix of
        - automation using content `from main config file`
        - copying contents of local file `in config data directory`
    - user_gate_host_file: handled with
        - automation using content `from main config file`

- /ansible/roles/role_name/
    - defaults/main.yaml: handled with
        - automation using content `from main config file`
    - tasks/main.yaml: handled with mix of
        - automation `using no config`
        - copying contents of local files `in config data directory`
    - templates/ *all files*: handled with mix of
        - automation using content `from main config file`
        - copying contents of local file `in config data directory`
    - files/ *all files*: handled with mix of
        - automation using content `from main config file`
        - copying contents of local file `in config data directory`

### Automate

There is a `hefty amount of boiler plate and repetitive steps` in my workflow for adding new services to the project. Before all of that had to be `added by hand`.

Now I just add the new details specific to the service being added and `the repetitive work is done automatically`.

### Responsibility Separation

For awhile my host_vars files have been growing `monoliths` of config code that is `service agnostic` or related to a `bunch of different services`.

With the approach this tool brings I no longer am dealing with those monoliths as it all is broken down into `separate code per` `service agnostic` and `each service`. This way trying to edit just one piece of all this becomes much easier and bugs can be better isolated.

### Version Control & CI (Continuous Integration)

Before because of how spread out service particular configuration was, maintaining separate configuration version especially across different roles would mean exponentially more files, complexity, and pain. With this tool maintaining separate versions is as simple as multiple different config data directories.

Tracking changes in config related to a specific service is also much easier with this tool.

This tool contributes to me building out CI for this project in the future because this is the first step of programmatic deterministic config sync.

### Onboarding

If someone tried to build off my Homelab project here on GitHub as a foundation with the services they want to use it would have `taken awhile` due to `complexity` and `the amoung of work`.

That process is now `simple` and `quicker`.

---

## Examples - Base Configs Without Added Service-Related Content

#### Base Host Var File w/ No Services Added Yet

In yaml file in this directory.

Traefik_Tailscale network and Traefik block remain because they are neccasary before anything else gets added for all hosts making it part of the base.
Leaving Tailscale config comment because this is connected to the Traefik_Tailscale network block and should be paired with that.

User Gate not included because this is not part of the base of all host var files.

#### Base Service Tasks w/ No Service Specific Tasks Added Yet

pre-run boiler plate: var and file setup

post-steps boiler plate: deploy docker compose


## What this does not cover.

vlan work on router to match macvlan network setups //TODO: finish this doc piece