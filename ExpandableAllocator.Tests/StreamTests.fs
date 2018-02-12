module StreamTests

open ExpandableAllocator

open FsUnitTyped
open NUnit.Framework

open System.IO


let newStream(protection, size: int64) = new ExpandableStream(Allocator.Create(protection, size), true)


[<Test>]
let ``should not allow write on read-only stream``() =
    use stream = newStream(Protection.Read, 1024L)

    stream.CanRead |> shouldEqual true
    stream.CanWrite |> shouldEqual false

[<Test>]
let ``should allow write on read-write stream``() =
    use stream = newStream(Protection.ReadWrite, 1024L)

    stream.CanRead |> shouldEqual true
    stream.CanWrite |> shouldEqual true


[<TestCase("This is a short string.")>]
[<TestCase("This is a longer string that will require the actual size of the stream to change.")>]
let ``should write and read correctly``(input: string) =
    use stream = newStream(Protection.ReadWrite, 0x10_000L)

    stream.Allocator.ActualSize <- nativeint 25

    do
        use writer = new StreamWriter(stream)

        writer.Write(input)

    stream.Seek(0L, SeekOrigin.Begin) |> ignore

    do
        use reader = new StreamReader(stream)

        reader.ReadToEnd() |> shouldEqual input
