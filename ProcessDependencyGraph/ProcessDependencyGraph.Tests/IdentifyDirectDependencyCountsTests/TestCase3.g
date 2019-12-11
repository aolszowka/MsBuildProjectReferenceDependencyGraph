digraph {
// This test case shows that removing C will remove only 1 Project and removing
// B will also only remove 1 project. This is because D is referenced by both C
// and B therefore you cannot remove D unless both projects are removed.
A -> B
A -> C
C -> D
B -> D
B -> E
// In addition this test case will also show that coloring on E will be
// identical to B, whereas the coloring on D will be "White" because it is
// referenced by both projects
}
