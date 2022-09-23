File List:

RevitAPI.dll
RevitAPI.xml
RevitAPIUI.dll
RevitAPIUI.xml
AdWindows.dll
UIFramework.dll

Sets the references 'Copy Local' to False.

Example packages.config file content for multiple version filtering:
(Considers that your build configurations have the 'year' version of Revit in them.)


[snip]
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Revit_All_Main_Versions_API_x64" version="2011.0.0" targetFramework="net35"  allowedVersions="[2011.0.0,2012.0.0)" Condition="$(Configuration.Contains('2011'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2012.0.0" targetFramework="net40"  allowedVersions="[2012.0.0,2013.0.0)" Condition="$(Configuration.Contains('2012'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2013.0.0" targetFramework="net40"  allowedVersions="[2013.0.0,2014.0.0)" Condition="$(Configuration.Contains('2013'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2014.0.0" targetFramework="net40"  allowedVersions="[2014.0.0,2015.0.0)" Condition="$(Configuration.Contains('2014'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2015.0.0" targetFramework="net45"  allowedVersions="[2015.0.0,2016.0.0)" Condition="$(Configuration.Contains('2015'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2016.0.0" targetFramework="net45"  allowedVersions="[2016.0.0,2017.0.0)" Condition="$(Configuration.Contains('2016'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2017.0.0" targetFramework="net46"  allowedVersions="[2017.0.0,2018.0.0)" Condition="$(Configuration.Contains('2017'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2018.0.0" targetFramework="net46"  allowedVersions="[2018.0.0,2019.0.0)" Condition="$(Configuration.Contains('2018'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2019.0.0" targetFramework="net47"  allowedVersions="[2019.0.0,2020.0.0)" Condition="$(Configuration.Contains('2019'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2020.0.0" targetFramework="net47"  allowedVersions="[2020.0.0,2021.0.0)" Condition="$(Configuration.Contains('2020'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2021.0.0" targetFramework="net48"  allowedVersions="[2021.0.0,2022.0.0)" Condition="$(Configuration.Contains('2021'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2022.0.0" targetFramework="net48"  allowedVersions="[2022.0.0,2023.0.0)" Condition="$(Configuration.Contains('2022'))"/>
  <package id="Revit_All_Main_Versions_API_x64" version="2023.0.0" targetFramework="net48"  allowedVersions="[2023.0.0,2024.0.0)" Condition="$(Configuration.Contains('2023'))"/>
</packages>
[/snip]

Example excerpt from a .vbproj file for Revit 2012/2013/2014 (they are all .NET 4.0):
(Note the net40 will vary across other versions, and the hint path will vary when updates
are applied, and due to the location of your NUGET 'packages' storage location.)

    <Reference Include="AdWindows"  Condition="$(Configuration.Contains('2012'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2012.0.0\lib\net40\AdWindows.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="RevitAPI"  Condition="$(Configuration.Contains('2012'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2012.0.0\lib\net40\RevitAPI.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="RevitAPIUI"  Condition="$(Configuration.Contains('2012'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2012.0.0\lib\net40\RevitAPIUI.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="UIFramework"  Condition="$(Configuration.Contains('2012'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2012.0.0\lib\net40\UIFramework.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="AdWindows"  Condition="$(Configuration.Contains('2013'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2013.0.0\lib\net40\AdWindows.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="RevitAPI"  Condition="$(Configuration.Contains('2013'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2013.0.0\lib\net40\RevitAPI.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="RevitAPIUI"  Condition="$(Configuration.Contains('2013'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2013.0.0\lib\net40\RevitAPIUI.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
	<Reference Include="UIFramework"  Condition="$(Configuration.Contains('2013'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2013.0.0\lib\net40\UIFramework.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="AdWindows"  Condition="$(Configuration.Contains('2014'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2014.0.0\lib\net40\AdWindows.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="RevitAPI"  Condition="$(Configuration.Contains('2014'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2014.0.0\lib\net40\RevitAPI.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="RevitAPIUI"  Condition="$(Configuration.Contains('2014'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2014.0.0\lib\net40\RevitAPIUI.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>
    <Reference Include="UIFramework"  Condition="$(Configuration.Contains('2014'))">
      <HintPath>..\Packages\Revit_All_Main_Versions_API_x64.2014.0.0\lib\net40\UIFramework.dll</HintPath>
      <SpecificVersion>False</SpecificVersion>
      <Private>False</Private>
    </Reference>