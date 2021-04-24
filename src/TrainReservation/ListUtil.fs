namespace TrainReservation

open System.Collections.Generic

module ListUtil =

    let rec remove_first pred lst =
        match lst with
        | h :: t when pred h -> t
        | h :: t -> h :: remove_first pred t
        | _ -> []

    let rec insert_after elem newE lst =
        match lst with
        | [] -> [ newE ]
        | h :: t ->
            if t = elem then
                (h :: newE :: t)
            else
                h :: (insert_after elem newE t)

    let rec update_element pred elem lst =
        match lst with
        | h :: t when pred h -> elem :: t
        | h :: t -> h :: update_element pred elem t
        | _ -> []

    let rec find_element pred lst =
        match lst with
        | h :: _ when pred h -> h
        | h :: t -> find_element pred t
        | _ -> raise (KeyNotFoundException $"predicate did not match: {pred}")
