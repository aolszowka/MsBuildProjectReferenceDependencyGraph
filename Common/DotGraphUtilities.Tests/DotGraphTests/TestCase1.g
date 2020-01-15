digraph {
// This is testing a fairly complex digraph; it covers many corner cases
// This is a Comment
A [style = filled, fillcolor = green, fontname="consolas", fontcolor=black]
A -> B
B [style = filled, fillcolor = red, fontname="consolas", fontcolor=black]
B -> C
A -> D
D -> B
C
A -> C
labelloc="t"
label="Dependencies"
}
