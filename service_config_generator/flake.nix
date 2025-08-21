{
	description = "My Homelab IaC dev environment.";
	inputs = {
		nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
		flake-utils.url = "github:numtide/flake-utils";
	};

	outputs = {
		self,
		nixpkgs,
		flake-utils,
		...
	}:
	flake-utils.lib.eachDefaultSystem (system:
		let
			pkgs = import nixpkgs {
				inherit system;
				pkgs = nixpkgs.legacyPackages.${system};
				config.allowUnfree = true;
			};
		in rec {
			devShell = pkgs.mkShell {
				packages = with pkgs; [
                    # note: must the cmd 'code .' from this shell
					vscode-fhs # needed for C# Extensions & Debugging
					dotnetCorePackages.dotnet_9.sdk # includes the runtime
				];

				shellHook = ''
					export DOTNET_ROOT="${pkgs.dotnetCorePackages.dotnet_9.sdk}/share/dotnet"
					export DOTNET_BIN="${pkgs.dotnetCorePackages.dotnet_9.sdk}/bin/dotnet"
				'';
			};
		}
	);
}