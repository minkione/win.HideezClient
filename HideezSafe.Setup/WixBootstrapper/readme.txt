Инструкция к созданию новой сборки Hideez Safe

- изменить версию в Version.cs
- изменить версию в Include.wxi
- очистить solution
- собрать x86 релиз WiXSetup
- собрать x64 релиз WiXSetup
- удостовериться, что у WixBootstrapper нет зависимости от WixSetup перед сборкой
- собрать х64 релиз WiXBootstrapper

Passing argument to bootstrapper
	hideezsafe.exe HESAddress=https://HostName InstallDongleDriver=1 IgnoreWorkstationOwnershipSecurity=0

Passing argument to MSI
	hideezsafe.msi HESADDRESS=https://HostName INSTALLDONGLEDRIVER=1 IGNOREWORKSTATIONOWNERSHIPSECURITY=0