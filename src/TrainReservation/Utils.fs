namespace TrainReservation

module Utils =

    open System.IO

    /// <summary>Active pattern for matching empty sequence</summary>
    /// <code>let s0 = Seq.empty&lt;int&gt;
    ///       match s0 with
    ///       | EmptySeq -> "empty"
    ///       | _ -> "not empty"
    /// </code>
    let (|EmptySeq|_|) a = if Seq.isEmpty a then Some() else None

    /// <summary>Return contents of a fixture as string. Ending newline is stripped of</summary>
    /// <param name="relativePath">relative path to fixture from project root</param>
    /// <returns>file contents as string</returns>
    let readFile relativePath =
        let content = File.ReadAllText(relativePath)

        if content.EndsWith System.Environment.NewLine then
            content.TrimEnd(System.Environment.NewLine.ToCharArray())
        else
            content

    /// <summary>Use in combination with below 'is' function to determine if an object 'is of Type x'</summary>
    /// <code>is (function Available -> true | _ -> false) obj</code>
    /// https://stackoverflow.com/questions/13070487/f-use-generic-type-as-pattern-discriminator
    let is cond item = if cond item then true else false
