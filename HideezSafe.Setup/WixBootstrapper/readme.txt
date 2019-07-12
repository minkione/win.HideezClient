Инструкция к созданию новой сборки Hideez Safe

- изменить версию в Version.cs
- изменить версию в Include.wxi
- очистить solution
- собрать x86 релиз WiXSetup
- собрать x64 релиз WiXSetup
- удостовериться, что у WixBootstrapper нет зависимости от WixSetup перед сборкой
- собрать х64 релиз WiXBootstrapper

Passing argument to bootstrapper
	hideezsafe.exe HostServerAddress=https://HostName InstallDongleDriver=1 BypassWorkstationOwnershipSecurity=0

Passing argument to MSI
	hideezsafe.msi HOSTSERVERADDRESS=https://HostName INSTALLDONGLEDRIVER=1 BYPASSWORKSTATIONOWNERSHIPSECURITY=0