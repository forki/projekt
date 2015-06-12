#load "Util.fs"
#load "Types.fs"
#r "System.Xml"
#r "System.Xml.Linq"

open System
open System.Xml.Linq

let xns s = XNamespace.Get s
let msbuildns = "{http://schemas.microsoft.com/developer/msbuild/2003}"
let xname s = XName.Get s

let xn s = XName.Get s
let xe n (v: obj) = new XElement(xn (msbuildns + n), v)
let xa n (v: obj) = new XAttribute(xn n, v)

let (|Head|_|) =
    Seq.tryFind (fun _ -> true)

let (|Value|) (xe: XElement) =
    xe.Value

let (|Guid|_|) s =
    match Guid.TryParse s with
    | true, g -> Some g
    | _ -> None

let (|Descendant|_|) name (xe : XElement) =
    match xe.Descendants (xn (msbuildns + name)) with
    | Head h -> Some h
    | _ -> None

let (|Element|_|) name (xe : XElement) =
    match xe.Element (xn (msbuildns + name)) with
    | null -> None 
    | e -> Some e

//getters
let projectGuid = 
    function
    | Descendant "ProjectGuid" (Value (Guid pg)) -> 
        Some pg 
    | _ -> None

let projectName = 
    function
    | Descendant "Name" (Value name) -> 
        Some name 
    | _ -> None

let projectReferenceItemGroup =
    function
    | Descendant "ProjectReference" e -> 
        e.Parent |> Some
    | _ -> None

let projRefT = """
    <ProjectReference Include="..\..\src\Projekt\Projekt.fsproj">
      <Name>Projekt</Name>
      <Project>{165a6853-05ed-4f03-a7b1-1c84d4f01bf5}</Project>
      <Private>True</Private>
    </ProjectReference>
    """

let addProjRefNode (path: string) (name: string) (guid : Guid) (el: XElement) =
    match projectReferenceItemGroup el with
    | Some prig ->
        prig.Add(
            xe "ProjectReference"
                [ xa "Include" path |> box
                  xe "Name" name |> box
                  xe "Project" (sprintf "{%O}" <| guid) |> box
                  xe "Private" "True" |> box ] )
        prig
    | None -> failwith "not yet"

let p = XElement.Load("Projekt.fsproj")
let pt = XElement.Load("../../tests/Projekt.Tests/Projekt.Tests.fsproj")
addProjRefNode "..\..\src\Project.fsproj" "Testing" (Guid.NewGuid()) pt
pt

IO.Path.GetFullPath "Project.fsproj"

let addReference (project : string) (reference : string) =
    let relPath = Projekt.Util.makeRelativePath project reference
    let proj = XElement.Load project
    let reference = XElement.Load reference
    let name = projectName reference
    let guid = projectGuid reference
    addProjRefNode relPath name.Value guid.Value proj
    
    




