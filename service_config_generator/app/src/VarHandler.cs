using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;

namespace ServiceConfigGenerator.src
{
    public class VarHandler
    {
        public readonly static string defaultServiceVars = """
            docker_dir: "/home/ansible/docker"
            project_name: "searxng"
            domain: ""

            network_backend:
            name: ""
            ipam_options:
            - subnet: ""
                gateway: ""
            traefik_ips:
                traefik: "172.80.2.2"
                searxng: "172.80.2.3"
        """;

        public readonly static string defaultHostServiceVars = """
            cloud_searxng:
            name: "searxng"
            domain: "searxng.cloud.nerolab.dev"
            network_backend:
                name: "t3_searxng"
                ipam_options:
                - subnet: "10.110.31.0/29"
                gateway: "10.110.31.1"
                traefik_ips:
                traefik: "10.110.31.3"
                searxng: "10.110.31.5"
        """;

        public readonly static string defaultHostNetworkVars = """
            cloud_networks:
            shared:
            # traefik tailscale
                - name: "traefik_tailscale"
                ipam_options:
                - subnet: "10.110.0.0/29"
                    gateway: "10.110.0.1"
                traefik_ip: "10.110.0.2"
                tailscale_ip: "10.110.0.4"
                dns_ip: "10.110.0.5"
            # monitor center
                - name: "{{cloud_monitor_center.network_backend.name}}"
                ipam_options: "{{cloud_monitor_center.network_backend.ipam_options}}"
                traefik_ip: "{{cloud_monitor_center.network_backend.ips.traefik}}"
                tailscale_ip: "{{cloud_monitor_center.network_backend.ips.tailscale}}"
            traefik_specific: # these containers don't have Tailscale DNS and connection-ability
            # searxng
                - name: "{{cloud_searxng.network_backend.name}}"
                ipam_options: "{{cloud_searxng.network_backend.ipam_options}}"
                traefik_ip: "{{cloud_searxng.network_backend.traefik_ips.traefik}}"
            # vikunja
                - name: "{{cloud_vikunja.network_backend.name}}"
                ipam_options: "{{cloud_vikunja.network_backend.ipam_options}}"
                traefik_ip: "{{cloud_vikunja.network_backend.traefik_ips.traefik}}"
            # expenseowl
                - name: "{{cloud_expenseowl.network_backend.name}}"
                ipam_options: "{{cloud_expenseowl.network_backend.ipam_options}}"
                traefik_ip: "{{cloud_expenseowl.network_backend.traefik_ips.traefik}}"
            # edgeshark
                - name: "{{cloud_edgeshark.network_backend.name}}"
                ipam_options: "{{cloud_edgeshark.network_backend.ipam_options}}"
                traefik_ip: "{{cloud_edgeshark.network_backend.traefik_ips.traefik}}"
            tailscale_specific: ""
        """;

        public readonly static string defaultHostUserGateVars = """
            gate_traefik_instances:
            cloud:
                traefik_ip: 10.110.0.2
                domains_list:
                - name: traefik.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                - name: nextcloud.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                - name: searxng.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                    - 100.122.229.69 # starfief-windows
                - name: vikunja.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                    - 100.122.229.69 # starfief-windows
                - name: expenseowl.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                    - 100.122.229.69 # starfief-windows
                - name: grafana.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                - name: loki.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                - name: alloy.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                - name: edgeshark.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                - name: cadvisor.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                - name: mimir.cloud.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
            media:
                traefik_ip: 10.130.0.2
                domains_list:
                - name: traefik.media.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                - name: searxng.media.nerolab.dev
                    allowed_tailscale_client_ips:
                    - 100.93.170.126 # stardom
                    - 100.122.229.69 # starfief-windows
        """;

        public static void AnsibleVarsCheck()
        {
            // if (config.ProjectName == "" || config.ProjectName.Contains(' '))
            // {
            //     throw new Exception("Config Validator Error: Project name invalid.");
            // }

            // string[] networkSetups = ["traefik_only", "tailscale_only", "shared"];
            // bool netSetupValid = false;
            // foreach (string netSetup in networkSetups)
            // {
            //     if (netSetup == config.NetworkSetup) netSetupValid = true;
            // }
            // if (netSetupValid == false)
            // {
            //     throw new Exception("Config Validator Error: NetworkSetup invalid.");
            // }
        }

        public static void AnsibleVarsInsert()
        {
            //
        }

        public static void ComposeCheck()
        {
            //
        }

        public static void ComposeCopy()
        {
            //
        }

    }
}