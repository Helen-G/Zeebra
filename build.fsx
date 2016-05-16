//#region F# Regions outlining extension download link
(*
https://visualstudiogallery.msdn.microsoft.com/bec977b8-c9d9-4926-999e-e50c4498df8a
*)
//#endregion

//#region Includes
#r @"libs\FAKE\tools\FakeLib.dll"
#r "System.Xml.dll"
#r "System.Xml.Linq.dll"
#r "System.IO.Compression.FileSystem.dll"
#r @"libs\FSharp.Collections.ParallelSeq\lib\net40\FSharp.Collections.ParallelSeq.dll"

open Fake
open Fake.DotCover
open System.Xml
open System.Xml.Linq
open System
open System.IO
open System.Linq
open System.Diagnostics
open System.Collections.Generic
open System.Net
open FSharp.Collections.ParallelSeq
//#endregion

let EnumClassToString = 
    fun obj -> 
        obj
            .GetType()
            .GetFields(System.Reflection.BindingFlags.Static|||System.Reflection.BindingFlags.NonPublic)
            .FirstOrDefault(fun f -> obj.Equals(f.GetValue(obj))).Name
            .Split([|"_unique_"|],StringSplitOptions.None).[1]

//#region Build Parameters
type BuildConfiguration = 
    | Develop
    | QA
    | DevelopSelenium
    | DevelopSeleniumTwice
    | IntegrationDev1
    | IntegrationDev2
    | IntegrationDev3
    | IntegrationStaging1
    | IntegrationStaging2
    | IntegrationStaging3
    | Agent1
    | Agent2
    | Agent3
    | Agent4
    | Agent5
    | UIUX
    | Load
    | Soak
    | NA

    override this.ToString() = EnumClassToString(this)

type BuildStep = 
    | StopWinService
    | PackageRestore
    | Build
    | DeployWinServiceBinaries
    | DropDatabase
    | UpdateTestsConfig
    | UpdateAdminApiTestsConfig
    | UpdateGameApiTestsConfig
    | RunUnitTests
    | RunIntegrationTests
    | RunSmokeTests
    | RunSeleniumTests
    | RunSeleniumTestsTwice
    | RunLoadTests
    | RunSoakTests
    | GenerateJMeterIndex
    | PublishAdminApi
    | PublishMemberApi
    | PublishGameApi
    | PublishMemberWebsite
    | PublishAdminWebsite
    | PublishGameWebsite
    | PingWebsites
    | StartWinService
    | ReportBuildStatus
    | GenerateScreenshotsIndex
    | GenerateLogsIndex
    | ActivateFinalTargets

    override this.ToString() = EnumClassToString(this)

let buildType = "Debug"
let isProductionMode = "false" // TODO: Later set this parameter dynamically
let MSDeployComputerName = @"https://regov2deploy.flycowdev.com:8172/msdeploy.axd"
let MSDeployPassword = "ch6kaCru"
let MSDeployUserName = @"WIN-NKD9IS8A8GG\robot"
let databaseUrl = "regov2db.flycowdev.com"
let rmqLogin = "regov2"
let rmqPassword = "!YTG9sd$A5q"
let rmqUrl = databaseUrl
let rmqPort = "5672"
let domain = ".flycowdev.com"
//#endregion

//#region Modules
module Configuration = 
    let make (configuration : string) = 
        match configuration with
            | "Develop" -> BuildConfiguration.Develop
            | "QA" -> BuildConfiguration.QA
            | "DevelopSelenium" -> BuildConfiguration.DevelopSelenium
            | "DevelopSeleniumTwice" -> BuildConfiguration.DevelopSeleniumTwice
            | "IntegrationDev1" -> BuildConfiguration.IntegrationDev1
            | "IntegrationDev2" -> BuildConfiguration.IntegrationDev2
            | "IntegrationDev3" -> BuildConfiguration.IntegrationDev3
            | "IntegrationStaging1" -> BuildConfiguration.IntegrationStaging1
            | "IntegrationStaging2" -> BuildConfiguration.IntegrationStaging2
            | "IntegrationStaging3" -> BuildConfiguration.IntegrationStaging3
            | "Agent1" -> BuildConfiguration.Agent1
            | "Agent2" -> BuildConfiguration.Agent2
            | "Agent3" -> BuildConfiguration.Agent3
            | "Agent4" -> BuildConfiguration.Agent4
            | "Agent5" -> BuildConfiguration.Agent5
            | "UIUX" -> BuildConfiguration.UIUX
            | "Load" -> BuildConfiguration.Load
            | "Soak" -> BuildConfiguration.Soak
            | _ -> BuildConfiguration.NA
//            | configuration -> failwith "Can not find proper build configuration!"

    let agentName = getBuildParam "agentName"

    let agentHomeDir = environVar "AgentHomeDir"

    let buildNumber = environVar "BUILD_NUMBER"

    let buildTarget = getBuildParam "buildTarget"

    let buildStepRepeatCount = System.Int32.Parse(getBuildParam "buildStepRepeatCount")

    let getCurrent = if (agentName.Equals("NA") || String.IsNullOrEmpty(agentName))
                        then buildTarget |> make
                        else agentName |> make

    let getPrefix configuration = 
        match configuration with
            | BuildConfiguration.Develop -> "dev"
            | BuildConfiguration.QA -> "qa"
            | BuildConfiguration.DevelopSelenium -> "dev-selenium"
            | BuildConfiguration.DevelopSeleniumTwice -> "dev-selenium-twice"
            | BuildConfiguration.IntegrationDev1 -> "integration-dev-1"
            | BuildConfiguration.IntegrationDev2 -> "integration-dev-2"
            | BuildConfiguration.IntegrationDev3 -> "integration-dev-3"
            | BuildConfiguration.IntegrationStaging1 -> "integration-staging-1"
            | BuildConfiguration.IntegrationStaging2 -> "integration-staging-2"
            | BuildConfiguration.IntegrationStaging3 -> "integration-staging-3"
            | BuildConfiguration.Agent1 -> "agent-1"
            | BuildConfiguration.Agent2 -> "agent-2"
            | BuildConfiguration.Agent3 -> "agent-3"
            | BuildConfiguration.Agent4 -> "agent-4"
            | BuildConfiguration.Agent5 -> "agent-5"
            | BuildConfiguration.UIUX -> "uiux"
            | BuildConfiguration.Load -> "load"
            | BuildConfiguration.Soak -> "soak"
            | BuildConfiguration.NA -> ""

    let getConfigurationFolderName configuration = 
        let buildConfiguration = configuration |> make
        match buildConfiguration with
            | BuildConfiguration.Develop -> "develop-unit"
            | BuildConfiguration.QA -> "qa"
            | BuildConfiguration.DevelopSelenium -> "develop-selenium"
            | BuildConfiguration.DevelopSeleniumTwice -> "develop-selenium-twice"
            | BuildConfiguration.NA -> "develop-selenium-twice"
            | buildConfiguration -> "other"

    let getScreenshotsPath = sprintf "C:\Projects\screenshots\%s\%s" (getConfigurationFolderName buildTarget) buildVersion

    let getLogsPath = @"C:\RegoV2Logs"

    let getWinServiceUrl configuration = 
        match configuration with
            | BuildConfiguration.Develop -> @"http://localhost:3345"
            | BuildConfiguration.QA -> @"http://localhost:3347"
            | BuildConfiguration.DevelopSelenium -> @"http://localhost:3348"
            | BuildConfiguration.DevelopSeleniumTwice -> @"http://localhost:3350"
            | BuildConfiguration.IntegrationDev1 -> @"http://localhost:3351"
            | BuildConfiguration.IntegrationDev2 -> @"http://localhost:3352"
            | BuildConfiguration.IntegrationDev3 -> @"http://localhost:3353"
            | BuildConfiguration.IntegrationStaging1 -> @"http://localhost:3354"
            | BuildConfiguration.IntegrationStaging2 -> @"http://localhost:3355"
            | BuildConfiguration.IntegrationStaging3 -> @"http://localhost:3356"
            | BuildConfiguration.Agent1 -> @"http://localhost:3361"
            | BuildConfiguration.Agent2 -> @"http://localhost:3362"
            | BuildConfiguration.Agent3 -> @"http://localhost:3363"
            | BuildConfiguration.Agent4 -> @"http://localhost:3364"
            | BuildConfiguration.Agent5 -> @"http://localhost:3369"
            | BuildConfiguration.UIUX -> @"http://localhost:3365"
            | BuildConfiguration.Load -> @"http://localhost:3367"
            | BuildConfiguration.Soak -> @"http://localhost:3368"
            | BuildConfiguration.NA -> ""

    let getWinServiceFullPath configuration = 
        sprintf @"C:\RegoV2Data-%s\WinService\WinService.exe" configuration

    let getRelativeLogFilePath projectName =
        sprintf @"\%s\%s\%s-log.txt" (getConfigurationFolderName buildTarget) buildVersion projectName

    let getWinServiceName configuration = 
        sprintf @"AFT.RegoV2.WinService.%s" configuration

    let getProjectIISName projectName configuration = 
        let configurationPrefix = getPrefix(configuration)
        match projectName with
            | "MemberApi" -> sprintf "%s-regov2-member-api" configurationPrefix
            | "AdminApi" -> sprintf "%s-regov2-admin-api" configurationPrefix
            | "GameApi" -> sprintf "%s-regov2-game-api" configurationPrefix
            | "MemberWebsite" -> sprintf "%s-regov2-member-website" configurationPrefix
            | "AdminWebsite" -> sprintf "%s-regov2-admin-website" configurationPrefix
            | "GameWebsite" -> sprintf "%s-regov2-game-website" configurationPrefix
            | projectName -> failwith "Can not find proper project IIS name for given project name!"

    let getFullWebsiteUrl projectName configuration = 
        let iisName = getProjectIISName projectName configuration
        sprintf @"http://%s%s/" iisName domain

    let getAllWebsitesUrls =
        let urls = new List<string>()
        urls.Add(getFullWebsiteUrl "AdminApi" getCurrent)
        urls.Add(getFullWebsiteUrl "MemberApi" getCurrent)
        urls.Add(getFullWebsiteUrl "GameApi" getCurrent)
        urls.Add(getFullWebsiteUrl "MemberWebsite" getCurrent)
        urls.Add(getFullWebsiteUrl "AdminWebsite" getCurrent)
        urls.Add(getFullWebsiteUrl "GameWebsite" getCurrent)
        urls

    let getDbConnectionString serverUrl =
        sprintf "Server=%s; Database=RegoV2-%s; Persist Security Info=True; integrated security=false; user id=sa; password=ch6kaCru" serverUrl (getPrefix(getCurrent))

module Command = 
    let execute cmd args dir = 
        let commandLine = sprintf "%s %s %s" cmd args dir

        printfn "Executing command line: %s" commandLine

        let errorCode = Shell.Exec(cmd, args, dir)

        if errorCode <> 0 then failwithf "Unable to execute script: %i" errorCode
            else printfn "Execution successful: %s" commandLine

    let executeRemotely command = 
        let cmd = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
        let args = sprintf "-verb:sync -source:runCommand -dest:runCommand=\"%s\",waitinterval=15000,ComputerName=\"%s\",UserName=\"%s\",Password=\"%s\",AuthType=\"Basic\" -allowUntrusted" command MSDeployComputerName MSDeployUserName MSDeployPassword
        let dir = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3"

        Shell.Exec(cmd, args, dir)

    let httpPing (url : string) = 
        try
            let request = WebRequest.Create(url);
            let response = request.GetResponse() :?> HttpWebResponse
            if (response.StatusCode.Equals(HttpStatusCode.OK))
                then sendStrToTeamCity (sprintf "HTTP Ping to %s is OK" url)
                else failwithf "HTTP Ping to %s has failed with the status code: %A!" url response.StatusCode
            response.Close()
        with
            | ex -> failwithf "HTTP Ping exception: %s" ex.Message

    let remoteFileExists (url : string) =
        let mutable result = false
        try
            let request = WebRequest.Create(url) :?> HttpWebRequest
            request.Method = "HEAD" |> ignore
            let response = request.GetResponse() :?> HttpWebResponse
            result <- response.StatusCode.Equals(HttpStatusCode.OK)
            response.Close()
        with
            | ex -> printfn "Remote file at %s exception: %s" url ex.Message
        result

module Settings = 
    let private xn s = XName.Get(s)

    let private updateConnectionString (xd : XDocument) connString =
        xd.Element(xn "configuration").Element(xn "connectionStrings").Elements(xn "add")
            |> Seq.iter (fun el -> if (el.Attribute(xn "name").Value.Equals("Default")) then el.SetAttributeValue(xn "connectionString", connString))

    let private updateAppSettings (xd : XDocument) (appSettings : Dictionary<string, string>) = 
        xd.Element(xn "configuration").Element(xn "appSettings").Elements(xn "add")
            |> Seq.iter (fun el -> let keyValue = el.Attribute(xn "key").Value
                                   if appSettings.ContainsKey(keyValue) then el.SetAttributeValue(xn "value", appSettings.[keyValue]))

    let private updateLogFilePath (xd : XDocument) logFile =
        xd.Element(xn "configuration").Element(xn "log4net").Element(xn "appender").Element(xn "file").SetAttributeValue(xn "value", logFile)

    let updateWebConfig appSettings (projectFileName : string) (webConfigPath : string) =
        let xd = XDocument.Load(webConfigPath)

        updateAppSettings xd appSettings

        if (projectFileName.Contains("MemberWebsite") |> not)
            then updateConnectionString xd (Configuration.getDbConnectionString databaseUrl)

        if (projectFileName |> String.IsNullOrEmpty |> not)
            then let logFile = Configuration.getLogsPath + Configuration.getRelativeLogFilePath projectFileName
                 updateLogFilePath xd logFile

        xd.Save(webConfigPath)

    let updateWinServiceConfig appSettings (winServiceConfigPath : string) =
        let xd = XDocument.Load(winServiceConfigPath)

        updateAppSettings xd appSettings

        updateConnectionString xd (Configuration.getDbConnectionString databaseUrl)

        let logFile = Configuration.getLogsPath + Configuration.getRelativeLogFilePath "WinService"
        updateLogFilePath xd logFile

        xd.Save(winServiceConfigPath)

    let updateJsConfigForAdminApi configPath apiUrl =
        let configContent = String.concat " " (ReadFile configPath)
        let updatedContent = replace "http://localhost:63684/" apiUrl configContent
        ReplaceFile configPath updatedContent

module IIS = 
    let private configurationNotExists iisName = 
        let cmd = @"C:\windows\system32\inetsrv\appcmd.exe"
        let args = sprintf "list site /name:%s" iisName
        let dir = ""

        Command.executeRemotely (sprintf "%s %s" cmd args) <> 0

    let private createConfiguration iisName = 
        let cmd = @"C:\windows\system32\inetsrv\appcmd.exe"

        let executeCommand args =
            Command.executeRemotely (sprintf "%s %s" cmd args) |> ignore

        executeCommand (sprintf "add apppool /name:%s" iisName)
        executeCommand (sprintf "add site /name:%s /bindings:http/*:80:%s%s /physicalpath:\"C:\inetpub\wwwroot\%s\"" iisName iisName domain iisName)
        executeCommand (sprintf "set app \"%s/\" /applicationPool:\"%s\"" iisName iisName)

    let private setWritable folderPath =
        let cmd = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
        let args = sprintf "-verb:sync -source:setacl -dest:setacl=\"%s\",setAclUser=\"IIS_IUSRS\",setAclAccess=Write,ComputerName=\"%s\",UserName=\"%s\",Password=\"%s\",AuthType=\"Basic\" -allowUntrusted" folderPath MSDeployComputerName MSDeployUserName MSDeployPassword
        let dir = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3"
        Command.execute cmd args dir

    let publishProject projectPath projectName appSettings = 
        let tempDir = currentDirectory @@ "tmp"
        let projectFileName = fileNameWithoutExt projectPath
        let packagePath = tempDir @@ (sprintf "%s.zip" projectFileName)
        let iisName = Configuration.getProjectIISName projectFileName Configuration.getCurrent

        if (configurationNotExists iisName) then createConfiguration iisName

        let getConfigPath tempDir projectFileName configFileName =
            let projectDir = Directory.GetDirectories(tempDir, projectFileName, SearchOption.AllDirectories) |> Seq.head
            let result = Directory.GetFiles(projectDir, configFileName, SearchOption.AllDirectories) |> Seq.head
            if (result.Length < 0) then failwithf "Config file not found!"
            result

        let cmd = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
        let args = sprintf @"%s /p:Configuration=%s /p:DeployOnBuild=true /P:ContinueOnError=false /P:AllowUntrustedCertificate=True /P:CreatePackageOnPublish=True /P:WebPublishPipelineProjectName=%s /P:DeployTarget=Package /p:PackageLocation=%s /P:VisualStudioVersion=12.0" projectPath buildType iisName packagePath
        let dir = ""
        Command.execute cmd args dir

        Unzip tempDir packagePath

        let webConfigPath = getConfigPath tempDir projectFileName "web.config"
        let sourcePath = Path.GetDirectoryName(webConfigPath)
        let destinationPath = @"C:\inetpub\wwwroot" @@ iisName

        Settings.updateWebConfig appSettings projectFileName webConfigPath

        if iisName.Contains("admin-website")
            then let jsConfigPath = getConfigPath tempDir projectFileName "config.js"
                 let sourcePath = Path.GetDirectoryName(jsConfigPath)
                 let adminApiUrl = Configuration.getFullWebsiteUrl "AdminApi" Configuration.getCurrent
                 Settings.updateJsConfigForAdminApi jsConfigPath adminApiUrl

        let cmd = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
        let args = sprintf "-verb:sync -source:contentPath=\"%s\" -dest:contentPath=\"%s\",ComputerName=\"%s\",UserName=\"%s\",Password=\"%s\",AuthType=\"Basic\" -allowUntrusted" sourcePath destinationPath MSDeployComputerName MSDeployUserName MSDeployPassword
        let dir = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3"
        Command.execute cmd args dir

        if iisName.Contains("admin-website")
            then setWritable (sprintf "C:\inetpub\wwwroot\%s\Uploads" iisName)

        CleanDir tempDir

module Tests = 
    let run includeCategory = 
        let testsDll = @"Tests\bin\" + buildType + @"\AFT.RegoV2.Tests.dll;
        Infrastructure\WebServices\GameApi.Tests\bin\" + buildType + @"\AFT.RegoV2.GameApi.Tests.dll;
        Infrastructure\WebServices\AdminApi.Tests\bin\" + buildType + @"\AFT.RegoV2.AdminApi.Tests.dll"

        let nunitLauncherPath = @"../../plugins/dotnetPlugin/bin/JetBrains.BuildServer.NUnitLauncher.exe"
        let nunitParams = sprintf "v4.0 x64 NUnit-2.6.3 /category-include:%s %s" includeCategory testsDll
        let dotCoverFileName = "DotCover.snapshot"
        let filters = "+:*AFT.RegoV2.Core*"

        let dotCoverParams = 
            (fun p -> { p with
                            ToolPath = @"../../tools/DotCover/dotcover.exe"
                            TargetExecutable = nunitLauncherPath
                            TargetArguments = nunitParams
                            TargetWorkingDir = currentDirectory
                            Output = dotCoverFileName
                            Filters = filters })

        let getUniqueName fileName =
            let rnd : string = (new System.Random()).Next(1, Int32.MaxValue).ToString()
            sprintf "%s%s" rnd fileName

        try
            DotCover dotCoverParams
        finally
            let randomFileName = getUniqueName dotCoverFileName
            Rename randomFileName dotCoverFileName
            sendToTeamCity "##teamcity[importData type='dotNetCoverage' tool='dotcover' path='%s']" randomFileName

module WinService = 
    let execute action = 
        let remoteCmd = 
            if (action.Equals("stop"))
                then sprintf "sc stop %s" (Configuration.getWinServiceName(Configuration.getCurrent.ToString()))
                else if (action.Equals("uninstall"))
                        then sprintf "sc delete %s" (Configuration.getWinServiceName(Configuration.getCurrent.ToString()))
                        else sprintf "%s %s" (Configuration.getWinServiceFullPath(Configuration.getCurrent.ToString())) action

        let cmd = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
        let args = sprintf "-verb:sync -source:runCommand -dest:runCommand=\"%s\",waitinterval=30000,ComputerName=\"%s\",UserName=\"%s\",Password=\"%s\",AuthType=\"Basic\" -allowUntrusted" remoteCmd MSDeployComputerName MSDeployUserName MSDeployPassword
        let dir = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3"

        if (action.Equals("stop") || action.Equals("uninstall"))
            then Shell.Exec(cmd, args, dir) |> ignore
            else Command.execute cmd args dir

//#endregion

//#region Build Steps
let mutable currentStep = ""

Target (BuildStep.StopWinService.ToString()) (fun _ ->
    currentStep <- BuildStep.StopWinService.ToString()

    WinService.execute "stop"
    WinService.execute "uninstall"
)

let solutionPath = "RegoV2.sln"

Target (BuildStep.PackageRestore.ToString()) (fun _ ->
    currentStep <- BuildStep.PackageRestore.ToString()

    let cmd = @".nuget\NuGet.exe"
    let args = sprintf @"restore %s" solutionPath
    let dir = ""

    Command.execute cmd args dir
)

Target (BuildStep.Build.ToString()) (fun _ ->
    currentStep <- BuildStep.Build.ToString()

    let targets = ["Clean"; "Build"]
    let toolsVersion = "12.0"

    let setParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = targets
            NodeReuse = false
            RestorePackagesFlag = false
            Properties =
                [
                    "Optimize", "True"
                    "DebugSymbols", "True"
                    "Configuration", buildType
                ]
            ToolsVersion = Some(toolsVersion)
        }

    build setParams solutionPath
        |> DoNothing
)

Target (BuildStep.DeployWinServiceBinaries.ToString()) (fun _ ->
    currentStep <- BuildStep.DeployWinServiceBinaries.ToString()

    let tempDir = currentDirectory @@ "tmp"
    let source = currentDirectory @@ "WinService" @@ "bin" @@ buildType
    let destination = DirectoryName (Configuration.getWinServiceFullPath(Configuration.getCurrent.ToString()))

    CopyDir tempDir source (fun _ -> true)

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("WinServiceUrl", (Configuration.getWinServiceUrl(Configuration.getCurrent)))
    appSettings.Add("WinServiceName", (sprintf @"AFT.RegoV2.WinService.%s" (Configuration.getCurrent.ToString())))
    appSettings.Add("rmq.login", rmqLogin)
    appSettings.Add("rmq.password", rmqPassword)
    appSettings.Add("rmq.url", rmqUrl)
    appSettings.Add("rmq.port", rmqPort)
    appSettings.Add("rmq.vhost", (Configuration.getPrefix(Configuration.getCurrent)))
    appSettings.Add("MockGameWebsite", (Configuration.getFullWebsiteUrl "GameWebsite" Configuration.getCurrent))
    appSettings.Add("MemberWebsiteUrl", (Configuration.getFullWebsiteUrl "MemberWebsite" Configuration.getCurrent))
    appSettings.Add("ProductionMode", isProductionMode)
    let winServiceConfigPath = tempDir @@ "WinService.exe.config"

    Settings.updateWinServiceConfig appSettings winServiceConfigPath

    let preSyncFilePath = currentDirectory @@ "PreSync.bat"
    let preSyncCommandLine = sprintf @"WMIC PROCESS WHERE ""ExecutablePath ='%s'"" delete" (Configuration.getWinServiceFullPath(Configuration.getCurrent.ToString()).Replace(@"\", @"\\"))
    System.IO.File.WriteAllText(preSyncFilePath, preSyncCommandLine)

    let cmd = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
    let args = sprintf "-verb:sync -preSync:runCommand=\"%s\",waitinterval=15000,successReturnCodes=0 -source:contentPath=\"%s\" -dest:contentPath=\"%s\",ComputerName=\"%s\",UserName=\"%s\",Password=\"%s\",AuthType=\"Basic\" -allowUntrusted" preSyncFilePath tempDir destination MSDeployComputerName MSDeployUserName MSDeployPassword
    let dir = @"C:\Program Files (x86)\IIS\Microsoft Web Deploy V3"

    Command.execute cmd args dir

    CleanDir tempDir
)

Target (BuildStep.DropDatabase.ToString()) (fun _ ->
    currentStep <- BuildStep.DropDatabase.ToString()

    let configurationPrefix = Configuration.getPrefix(Configuration.getCurrent)
    let DBName = sprintf "RegoV2-%s" configurationPrefix
    let DBUserName = "sa"
    let DBPassword = "ch6kaCru"
    let DBAddress = "regov2db.flycowdev.com"

    let cmd = @"sqlcmd"
    let args1 = sprintf "-S %s -U \"%s\" -P \"%s\" -q \"ALTER DATABASE [%s] SET SINGLE_USER WITH ROLLBACK IMMEDIATE\"" DBAddress DBUserName DBPassword DBName
    let args2 = sprintf "-S %s -U \"%s\" -P \"%s\" -q \"DROP DATABASE [%s]\"" DBAddress DBUserName DBPassword DBName
    let dir = ""

    Command.execute cmd args1 dir
    Command.execute cmd args2 dir
)

Target (BuildStep.UpdateTestsConfig.ToString()) (fun _ ->
    currentStep <- BuildStep.UpdateTestsConfig.ToString()
    
    let portNumber = 
        if ((Configuration.agentName).StartsWith("Agent"))
            then (Configuration.agentName.[Configuration.agentName.Length - 1].ToString())
            else "5"

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("MemberWebsiteUrl", (Configuration.getFullWebsiteUrl "MemberWebsite" Configuration.getCurrent))
    appSettings.Add("AdminWebsiteUrl", (Configuration.getFullWebsiteUrl "AdminWebsite" Configuration.getCurrent))
    appSettings.Add("MockGameWebsite", (Configuration.getFullWebsiteUrl "GameWebsite" Configuration.getCurrent))
    appSettings.Add("TestServerUri", ("http://localhost:555" + portNumber))
    appSettings.Add("rmq.login", rmqLogin)
    appSettings.Add("rmq.password", rmqPassword)
    appSettings.Add("rmq.url", rmqUrl)
    appSettings.Add("rmq.port", rmqPort)
    appSettings.Add("rmq.vhost", (Configuration.getPrefix(Configuration.getCurrent)))
    appSettings.Add("ServiceHostUrl", "http://localhost:10478/")
    appSettings.Add("ScreenshotsPath", (Configuration.getScreenshotsPath))

    let webConfigPath = currentDirectory @@ "Tests" @@ "bin" @@ buildType @@ "AFT.RegoV2.Tests.dll.config"

    Settings.updateWebConfig appSettings "" webConfigPath
)

Target (BuildStep.UpdateGameApiTestsConfig.ToString()) (fun _ ->
    currentStep <- BuildStep.UpdateGameApiTestsConfig.ToString()

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("GameApiUrl", (Configuration.getFullWebsiteUrl "GameApi" Configuration.getCurrent))
    appSettings.Add("MemberApiUrl", (Configuration.getFullWebsiteUrl "MemberApi" Configuration.getCurrent))

    let webConfigPath = currentDirectory @@ "Infrastructure" @@ "WebServices" @@ "GameApi.Tests" @@ "bin" @@ buildType @@ "AFT.RegoV2.GameApi.Tests.dll.config"

    Settings.updateWebConfig appSettings "" webConfigPath
)

Target (BuildStep.UpdateAdminApiTestsConfig.ToString()) (fun _ ->
    currentStep <- BuildStep.UpdateAdminApiTestsConfig.ToString()

    let adminApiUrl = Configuration.getFullWebsiteUrl "AdminApi" Configuration.getCurrent

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("AdminApiUrl", adminApiUrl)

    let webConfigPath = currentDirectory @@ "Infrastructure" @@ "WebServices" @@ "AdminApi.Tests" @@ "bin" @@ buildType @@ "AFT.RegoV2.AdminApi.Tests.dll.config"

    Settings.updateWebConfig appSettings "" webConfigPath
)

Target (BuildStep.RunUnitTests.ToString()) (fun _ ->
    currentStep <- BuildStep.RunUnitTests.ToString()

    let includeCategory = "Unit"
    Tests.run includeCategory
)

Target (BuildStep.RunIntegrationTests.ToString()) (fun _ ->
    currentStep <- BuildStep.RunIntegrationTests.ToString()

    let includeCategory = "Integration"
    Tests.run includeCategory
)

Target (BuildStep.RunSmokeTests.ToString()) (fun _ ->
    currentStep <- BuildStep.RunSmokeTests.ToString()

    let includeCategory = "Smoke"
    Tests.run includeCategory
)

Target (BuildStep.RunSeleniumTests.ToString()) (fun _ ->
    currentStep <- BuildStep.RunSeleniumTests.ToString()

    let includeCategory = "Selenium"
    Tests.run includeCategory
)

Target (BuildStep.RunSeleniumTestsTwice.ToString()) (fun _ ->
    currentStep <- BuildStep.RunSeleniumTestsTwice.ToString()

    let includeCategory = "Selenium"

    for i = 1 to (Configuration.buildStepRepeatCount) do
        try
            printfn "Running selenium tests (%i of %i)" i Configuration.buildStepRepeatCount
            Tests.run includeCategory
        with
            | _ -> ()
)

Target (BuildStep.PublishAdminApi.ToString()) (fun _ ->
    currentStep <- BuildStep.PublishAdminApi.ToString()

    let projectPath = @"Infrastructure\WebServices\AdminApi\AdminApi.csproj"
    let projectName = "AdminApi.csproj"

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("MockGameWebsite", (Configuration.getFullWebsiteUrl "GameWebsite" Configuration.getCurrent))
    appSettings.Add("rmq.login", rmqLogin)
    appSettings.Add("rmq.password", rmqPassword)
    appSettings.Add("rmq.url", rmqUrl)
    appSettings.Add("rmq.port", rmqPort)
    appSettings.Add("rmq.vhost", (Configuration.getPrefix(Configuration.getCurrent)))

    IIS.publishProject projectPath projectName appSettings
)

Target (BuildStep.PublishMemberApi.ToString()) (fun _ ->
    currentStep <- BuildStep.PublishMemberApi.ToString()

    let projectPath = @"Infrastructure\WebServices\MemberApi\MemberApi.csproj"
    let projectName = "MemberApi.csproj"

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("MockGameWebsite", (Configuration.getFullWebsiteUrl "GameWebsite" Configuration.getCurrent))
    appSettings.Add("rmq.login", rmqLogin)
    appSettings.Add("rmq.password", rmqPassword)
    appSettings.Add("rmq.url", rmqUrl)
    appSettings.Add("rmq.port", rmqPort)
    appSettings.Add("rmq.vhost", (Configuration.getPrefix(Configuration.getCurrent)))

    IIS.publishProject projectPath projectName appSettings
)

Target (BuildStep.PublishGameApi.ToString()) (fun _ ->
    currentStep <- BuildStep.PublishGameApi.ToString()

    let projectPath = @"Infrastructure\WebServices\GameApi\GameApi.csproj"
    let projectName = "GameApi.csproj"

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("MockGameWebsite", (Configuration.getFullWebsiteUrl "GameWebsite" Configuration.getCurrent))
    appSettings.Add("rmq.login", rmqLogin)
    appSettings.Add("rmq.password", rmqPassword)
    appSettings.Add("rmq.url", rmqUrl)
    appSettings.Add("rmq.port", rmqPort)
    appSettings.Add("rmq.vhost", (Configuration.getPrefix(Configuration.getCurrent)))

    IIS.publishProject projectPath projectName appSettings
)

Target (BuildStep.PublishMemberWebsite.ToString()) (fun _ ->
    currentStep <- BuildStep.PublishMemberWebsite.ToString()

    let projectPath = @"Presentation\MemberWebsite\MemberWebsite.csproj"
    let projectName = "MemberWebsite.csproj"

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("MemberApiUrl", (Configuration.getFullWebsiteUrl "MemberApi" Configuration.getCurrent))
    appSettings.Add("MockGameWebsite", (Configuration.getFullWebsiteUrl "GameWebsite" Configuration.getCurrent))
    appSettings.Add("rmq.login", rmqLogin)
    appSettings.Add("rmq.password", rmqPassword)
    appSettings.Add("rmq.url", rmqUrl)
    appSettings.Add("rmq.port", rmqPort)
    appSettings.Add("rmq.vhost", (Configuration.getPrefix(Configuration.getCurrent)))

    IIS.publishProject projectPath projectName appSettings
)

Target (BuildStep.PublishAdminWebsite.ToString()) (fun _ ->
    currentStep <- BuildStep.PublishAdminWebsite.ToString()

    let projectPath = @"Presentation\AdminWebsite\AdminWebsite.csproj"
    let projectName = "AdminWebsite.csproj"

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("AdminApiUrl", (Configuration.getFullWebsiteUrl "AdminApi" Configuration.getCurrent))
    appSettings.Add("MemberApiUrl", (Configuration.getFullWebsiteUrl "MemberApi" Configuration.getCurrent))
    appSettings.Add("MockGameWebsite", (Configuration.getFullWebsiteUrl "GameWebsite" Configuration.getCurrent))
    appSettings.Add("WinServiceUrl", (Configuration.getWinServiceUrl(Configuration.getCurrent)))
    appSettings.Add("rmq.login", rmqLogin)
    appSettings.Add("rmq.password", rmqPassword)
    appSettings.Add("rmq.url", rmqUrl)
    appSettings.Add("rmq.port", rmqPort)
    appSettings.Add("rmq.vhost", (Configuration.getPrefix(Configuration.getCurrent)))

    IIS.publishProject projectPath projectName appSettings
)

Target (BuildStep.PublishGameWebsite.ToString()) (fun _ ->
    currentStep <- BuildStep.PublishGameWebsite.ToString()

    let projectPath = @"Presentation\GameWebsite\GameWebsite.csproj"
    let projectName = "GameWebsite.csproj"

    let appSettings = new Dictionary<string, string>()
    appSettings.Add("GameApiUrl", (Configuration.getFullWebsiteUrl "GameApi" Configuration.getCurrent))
    appSettings.Add("MockGameWebsite", (Configuration.getFullWebsiteUrl "GameWebsite" Configuration.getCurrent))
    appSettings.Add("rmq.login", rmqLogin)
    appSettings.Add("rmq.password", rmqPassword)
    appSettings.Add("rmq.url", rmqUrl)
    appSettings.Add("rmq.port", rmqPort)
    appSettings.Add("rmq.vhost", (Configuration.getPrefix(Configuration.getCurrent)))

    IIS.publishProject projectPath projectName appSettings
)

Target (BuildStep.PingWebsites.ToString()) (fun _ ->
    currentStep <- BuildStep.PingWebsites.ToString()

    Configuration.getAllWebsitesUrls 
        |> PSeq.iter (fun url -> Command.httpPing url)
)

Target (BuildStep.StartWinService.ToString()) (fun _ ->
    currentStep <- BuildStep.StartWinService.ToString()

    WinService.execute "install"
    WinService.execute "start"
)

Target (BuildStep.RunLoadTests.ToString()) (fun _ ->
    currentStep <- BuildStep.RunLoadTests.ToString()

    let cmd = sprintf @"%s\plugins\ant\bin\ant.bat" (Configuration.agentHomeDir.ToString())
    let args = sprintf "-DbuildNumber=%s -DconfigurationName=%s" (Configuration.buildNumber.ToString()) (BuildConfiguration.Load.ToString())
    let dir = @"Tests\jMeter"

    Command.execute cmd args dir
)

Target (BuildStep.RunSoakTests.ToString()) (fun _ ->
    currentStep <- BuildStep.RunSoakTests.ToString()

    let cmd = sprintf @"%s\plugins\ant\bin\ant.bat" (Configuration.agentHomeDir.ToString())
    let args = sprintf "-DbuildNumber=%s -DconfigurationName=%s" (Configuration.buildNumber.ToString()) (BuildConfiguration.Soak.ToString())
    let dir = @"Tests\jMeter"

    Command.execute cmd args dir
)

Target (BuildStep.ActivateFinalTargets.ToString()) (fun _ ->
    currentStep <- BuildStep.ActivateFinalTargets.ToString()

    ActivateBuildFailureTarget (BuildStep.GenerateScreenshotsIndex.ToString())

    ActivateBuildFailureTarget (BuildStep.ReportBuildStatus.ToString())

    ActivateFinalTarget (BuildStep.GenerateLogsIndex.ToString())
)

Target (BuildStep.GenerateJMeterIndex.ToString()) (fun _ ->
    currentStep <- BuildStep.GenerateJMeterIndex.ToString()

    let artifactsDir = sprintf @"C:\RegoV2-Perf\%s\%s\html" (Configuration.getCurrent.ToString()) (Configuration.buildNumber.ToString())
    let getHtmlLink file = sprintf @"<li><a href=""%s"">%s</a></li>" file file
    let content = Directory.GetFiles(artifactsDir, "*.html") |> Array.map (fun file -> getHtmlLink (filename file)) |> String.concat ""
    let getIndexHtml content = sprintf "<!DOCTYPE html><html><body><h2>Tests results:</h2><ul>%s</ul></body></html>" content
    WriteStringToFile false (artifactsDir @@ "index.html") (getIndexHtml content)
)

BuildFailureTarget (BuildStep.GenerateScreenshotsIndex.ToString()) (fun _ ->
    if (currentStep.Equals(BuildStep.RunSmokeTests.ToString()) || 
        currentStep.Equals(BuildStep.RunSeleniumTests.ToString()) || 
        currentStep.Equals(BuildStep.RunSeleniumTestsTwice.ToString()))
        then let artifactsDir = Configuration.getScreenshotsPath
             let getHtmlLink file = sprintf @"<li><a href=""%s"">%s</a></li>" file file
             if (directoryExists artifactsDir) 
                 then let content = seq { yield! Directory.GetFiles(artifactsDir, "*.png") } |> Seq.map (fun file -> getHtmlLink (filename file)) |> String.concat ""
                      let getIndexHtml content = sprintf "<!DOCTYPE html><html><body><h2>Failed tests screenshots:</h2><ul>%s</ul></body></html>" content
                      WriteStringToFile false (artifactsDir @@ "index.html") (getIndexHtml content)
)

BuildFailureTarget (BuildStep.ReportBuildStatus.ToString()) (fun _ ->
    sendTeamCityError (sprintf "{build.status.text} in \"%s\" build step" currentStep)
)

FinalTarget (BuildStep.GenerateLogsIndex.ToString()) (fun _ ->
    let logsUrl = @"http://regov2logs.flycowdev.com"
    let projectsToLog = ["WinService"; "GameApi"; "AdminApi"; "MemberApi"; "AdminWebsite"; "GameWebsite"; "MemberWebsite"]
    let getHtmlLink linkName url = sprintf @"<li><a href=""%s"">%s</a></li>" url linkName
    let content = projectsToLog |> 
                    Seq.map (fun projectName -> 
                                    let logFileUrl = (logsUrl + (Configuration.getRelativeLogFilePath projectName).Replace(@"\", @"/"))
                                    if (Command.remoteFileExists logFileUrl)
                                        then getHtmlLink projectName logFileUrl
                                        else "") |> String.concat ""
    let getIndexHtml content = sprintf "<!DOCTYPE html><html><body><h2>Collected logs:</h2><ul>%s</ul></body></html>" content
    WriteStringToFile false (currentDirectory @@ "logs.html") (getIndexHtml content)
)

//#endregion Build Steps

//#region Build Configurations
Target (BuildConfiguration.Develop.ToString()) (fun _ ->
    let buildSteps =
        BuildStep.ActivateFinalTargets.ToString()
          ==> BuildStep.StopWinService.ToString()
          ==> BuildStep.PackageRestore.ToString()
          ==> BuildStep.Build.ToString()
          ==> BuildStep.DeployWinServiceBinaries.ToString()
          ==> BuildStep.DropDatabase.ToString()
          ==> BuildStep.UpdateTestsConfig.ToString()
          ==> BuildStep.UpdateAdminApiTestsConfig.ToString()
          ==> BuildStep.UpdateGameApiTestsConfig.ToString()
          ==> BuildStep.RunUnitTests.ToString()
          ==> BuildStep.PublishAdminApi.ToString()
          ==> BuildStep.PublishMemberApi.ToString()
          ==> BuildStep.PublishGameApi.ToString()
          ==> BuildStep.PublishMemberWebsite.ToString()
          ==> BuildStep.PublishAdminWebsite.ToString()
          ==> BuildStep.PublishGameWebsite.ToString()
          ==> BuildStep.StartWinService.ToString()
          ==> BuildStep.PingWebsites.ToString()
          ==> BuildStep.RunSmokeTests.ToString()

    RunTargetOrDefault (BuildStep.RunSmokeTests.ToString())
)

Target (BuildConfiguration.QA.ToString()) (fun _ ->
    let buildSteps =
        BuildStep.ActivateFinalTargets.ToString()
          ==> BuildStep.StopWinService.ToString()
          ==> BuildStep.PackageRestore.ToString()
          ==> BuildStep.Build.ToString()
          ==> BuildStep.DeployWinServiceBinaries.ToString()
          ==> BuildStep.DropDatabase.ToString()
          ==> BuildStep.UpdateTestsConfig.ToString()
          ==> BuildStep.UpdateAdminApiTestsConfig.ToString()
          ==> BuildStep.UpdateGameApiTestsConfig.ToString()
          ==> BuildStep.RunUnitTests.ToString()
          ==> BuildStep.PublishAdminApi.ToString()
          ==> BuildStep.PublishMemberApi.ToString()
          ==> BuildStep.PublishGameApi.ToString()
          ==> BuildStep.PublishMemberWebsite.ToString()
          ==> BuildStep.PublishAdminWebsite.ToString()
          ==> BuildStep.PublishGameWebsite.ToString()
          ==> BuildStep.StartWinService.ToString()
          ==> BuildStep.PingWebsites.ToString()
          ==> BuildStep.RunSeleniumTests.ToString()

    RunTargetOrDefault (BuildStep.RunSeleniumTests.ToString())
)

Target (BuildConfiguration.DevelopSelenium.ToString()) (fun _ ->
    let buildSteps =
        BuildStep.ActivateFinalTargets.ToString()
          ==> BuildStep.StopWinService.ToString()
          ==> BuildStep.PackageRestore.ToString()
          ==> BuildStep.Build.ToString()
          ==> BuildStep.DeployWinServiceBinaries.ToString()
          ==> BuildStep.DropDatabase.ToString()
          ==> BuildStep.UpdateTestsConfig.ToString()
          ==> BuildStep.UpdateAdminApiTestsConfig.ToString()
          ==> BuildStep.UpdateGameApiTestsConfig.ToString()
          ==> BuildStep.PublishAdminApi.ToString()
          ==> BuildStep.PublishMemberApi.ToString()
          ==> BuildStep.PublishGameApi.ToString()
          ==> BuildStep.PublishMemberWebsite.ToString()
          ==> BuildStep.PublishAdminWebsite.ToString()
          ==> BuildStep.PublishGameWebsite.ToString()
          ==> BuildStep.StartWinService.ToString()
          ==> BuildStep.PingWebsites.ToString()
          ==> BuildStep.RunIntegrationTests.ToString()
          ==> BuildStep.RunSeleniumTests.ToString()

    RunTargetOrDefault (BuildStep.RunSeleniumTests.ToString())
)

Target (BuildConfiguration.DevelopSeleniumTwice.ToString()) (fun _ ->
    let buildSteps =
        BuildStep.ActivateFinalTargets.ToString()
          ==> BuildStep.StopWinService.ToString()
          ==> BuildStep.PackageRestore.ToString()
          ==> BuildStep.Build.ToString()
          ==> BuildStep.RunUnitTests.ToString()
          ==> BuildStep.DeployWinServiceBinaries.ToString()
          ==> BuildStep.DropDatabase.ToString()
          ==> BuildStep.UpdateTestsConfig.ToString()
          ==> BuildStep.UpdateAdminApiTestsConfig.ToString()
          ==> BuildStep.UpdateGameApiTestsConfig.ToString()
          ==> BuildStep.PublishAdminApi.ToString()
          ==> BuildStep.PublishMemberApi.ToString()
          ==> BuildStep.PublishGameApi.ToString()
          ==> BuildStep.PublishMemberWebsite.ToString()
          ==> BuildStep.PublishAdminWebsite.ToString()
          ==> BuildStep.PublishGameWebsite.ToString()
          ==> BuildStep.StartWinService.ToString()
          ==> BuildStep.PingWebsites.ToString()
          ==> BuildStep.RunSeleniumTestsTwice.ToString()

    RunTargetOrDefault (BuildStep.RunSeleniumTestsTwice.ToString())
)

Target (BuildConfiguration.IntegrationDev1.ToString()) (fun _ ->
    let buildSteps =
        BuildStep.StopWinService.ToString()
          ==> BuildStep.PackageRestore.ToString()
          ==> BuildStep.Build.ToString()
          ==> BuildStep.DeployWinServiceBinaries.ToString()
          ==> BuildStep.DropDatabase.ToString()
          ==> BuildStep.PublishMemberApi.ToString()
          ==> BuildStep.PublishGameApi.ToString()
          ==> BuildStep.PublishMemberWebsite.ToString()
          ==> BuildStep.PublishAdminWebsite.ToString()
          ==> BuildStep.PublishGameWebsite.ToString()
          ==> BuildStep.StartWinService.ToString()

    RunTargetOrDefault (BuildStep.StartWinService.ToString())
)

Target (BuildConfiguration.IntegrationDev2.ToString()) (fun _ ->
    RunTargetOrDefault (BuildConfiguration.IntegrationDev1.ToString())
)

Target (BuildConfiguration.IntegrationDev3.ToString()) (fun _ ->
    RunTargetOrDefault (BuildConfiguration.IntegrationDev1.ToString())
)

Target (BuildConfiguration.IntegrationStaging1.ToString()) (fun _ ->
    RunTargetOrDefault (BuildConfiguration.IntegrationDev1.ToString())
)

Target (BuildConfiguration.IntegrationStaging2.ToString()) (fun _ ->
    RunTargetOrDefault (BuildConfiguration.IntegrationDev1.ToString())
)

Target (BuildConfiguration.IntegrationStaging3.ToString()) (fun _ ->
    RunTargetOrDefault (BuildConfiguration.IntegrationDev1.ToString())
)

Target (BuildConfiguration.UIUX.ToString()) (fun _ ->
    let buildSteps =
        BuildStep.ActivateFinalTargets.ToString()
          ==> BuildStep.StopWinService.ToString()
          ==> BuildStep.PackageRestore.ToString()
          ==> BuildStep.Build.ToString()
          ==> BuildStep.DeployWinServiceBinaries.ToString()
          ==> BuildStep.DropDatabase.ToString()
          ==> BuildStep.UpdateTestsConfig.ToString()
          ==> BuildStep.UpdateAdminApiTestsConfig.ToString()
          ==> BuildStep.UpdateGameApiTestsConfig.ToString()
          ==> BuildStep.PublishAdminApi.ToString()
          ==> BuildStep.PublishMemberApi.ToString()
          ==> BuildStep.PublishGameApi.ToString()
          ==> BuildStep.PublishMemberWebsite.ToString()
          ==> BuildStep.PublishAdminWebsite.ToString()
          ==> BuildStep.PublishGameWebsite.ToString()
          ==> BuildStep.StartWinService.ToString()
          ==> BuildStep.PingWebsites.ToString()
          ==> BuildStep.RunSmokeTests.ToString()

    RunTargetOrDefault (BuildStep.RunSmokeTests.ToString())
)


Target (BuildConfiguration.Load.ToString()) (fun _ ->
    let buildSteps =
        BuildStep.ActivateFinalTargets.ToString()
          ==> BuildStep.StopWinService.ToString()
          ==> BuildStep.PackageRestore.ToString()
          ==> BuildStep.Build.ToString()
          ==> BuildStep.DeployWinServiceBinaries.ToString()
          ==> BuildStep.DropDatabase.ToString()
          ==> BuildStep.PublishAdminApi.ToString()
          ==> BuildStep.PublishMemberApi.ToString()
          ==> BuildStep.PublishGameApi.ToString()
          ==> BuildStep.PublishMemberWebsite.ToString()
          ==> BuildStep.PublishAdminWebsite.ToString()
          ==> BuildStep.PublishGameWebsite.ToString()
          ==> BuildStep.StartWinService.ToString()
          ==> BuildStep.PingWebsites.ToString()
          ==> BuildStep.RunLoadTests.ToString()
          ==> BuildStep.GenerateJMeterIndex.ToString()

    RunTargetOrDefault (BuildStep.GenerateJMeterIndex.ToString())
)

Target (BuildConfiguration.Soak.ToString()) (fun _ ->
    let buildSteps =
        BuildStep.ActivateFinalTargets.ToString()
          ==> BuildStep.StopWinService.ToString()
          ==> BuildStep.PackageRestore.ToString()
          ==> BuildStep.Build.ToString()
          ==> BuildStep.DeployWinServiceBinaries.ToString()
          ==> BuildStep.DropDatabase.ToString()
          ==> BuildStep.PublishAdminApi.ToString()
          ==> BuildStep.PublishMemberApi.ToString()
          ==> BuildStep.PublishGameApi.ToString()
          ==> BuildStep.PublishMemberWebsite.ToString()
          ==> BuildStep.PublishAdminWebsite.ToString()
          ==> BuildStep.PublishGameWebsite.ToString()
          ==> BuildStep.StartWinService.ToString()
          ==> BuildStep.PingWebsites.ToString()
          ==> BuildStep.RunSoakTests.ToString()
          ==> BuildStep.GenerateJMeterIndex.ToString()

    RunTargetOrDefault (BuildStep.GenerateJMeterIndex.ToString())
)
//#endregion

RunTargetOrDefault (Configuration.buildTarget)