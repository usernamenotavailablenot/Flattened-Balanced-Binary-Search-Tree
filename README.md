# Flattened-BBST
This is an implementation of Flattened Balanced Binary Search Tree (FBBST), a hybrid of Balanced Binary Search Tree (BBST) and B-Tree supporting fast search, insertion, and deletion. The motivation is from B+ tree. The leaf level of B+ Tree is a sorted doubly linked list, and a doubly sorted linked list is equivalent to a Binary Search Tree (BST). Then, the leaf level of a B(+) tree can be converted to a BST, and the upper levels, which contains only indexes but no data, can be discarded. The BST can be balanced by techniques like AVL tree or Red-Black tree. In this implementation, Left Leaning Red Black (LLRB) tree is adopted.

The FBBST has following features from BST:
- Each node has exactly two children; 
- Balancing technique is the same.

The FBBST has following features from B-Tree: 
- Each node stores an array of elements; 
- Array has a lower and upper limits; 
- Insertion and deletion may involve node split and merge.

The advantages of FBBST: 
- more space efficient, since FBBST requires much fewer pointers.
- more cache friendly, since FBBST stores data in arrays.

The run time complexity is obviously `Log(N)` for search, insertion, and deletion. For benchmarking, the attached unit test instantiates 128 FBBST trees with different node capacity. Each FBBST is then performed with 100,000 random insertions and 100,000 random deletions. The whole test took around 30 seconds in my laptop with Intel(R) Core(TM) i7-3630QM CPU @ 2.40GHz and 8.00 GB Installed DDR3 RAM, which was purchased before 2015, with each FBBST tree spending 150-400 milliseconds. The attached unit test code runs the test above 10 times, and below is a sample output.

- min = 001 295 284 272 274 261 260 260 296 279 258
- min = 002 231 223 228 230 226 222 227 262 212 232
- min = 003 229 183 185 196 187 205 199 201 184 201
- min = 004 178 173 174 187 170 175 175 197 178 177
- min = 005 197 177 183 169 176 167 181 168 178 178
- min = 006 183 164 167 163 163 175 186 169 163 166
- min = 007 166 163 164 172 162 167 168 195 172 165
- min = 008 169 160 159 178 159 175 158 195 164 163
- min = 009 162 161 163 174 172 160 167 175 162 165
- min = 010 163 167 166 157 163 160 159 170 161 176
- min = 011 157 161 157 162 157 170 158 175 164 160
- min = 012 157 169 157 163 159 170 160 165 163 156
- min = 013 154 165 154 153 154 162 154 155 154 160
- min = 014 154 158 162 154 155 160 154 156 156 156
- min = 015 160 157 156 153 152 185 159 156 154 153
- min = 016 157 153 151 169 150 160 156 167 155 154
- min = 017 162 152 165 159 155 153 155 169 153 158
- min = 018 167 158 156 162 152 153 154 159 154 157
- min = 019 158 157 155 159 153 162 151 154 153 152
- min = 020 155 156 150 158 163 154 157 161 160 153
- min = 021 153 157 154 162 154 156 154 163 154 159
- min = 022 158 156 163 162 154 157 159 153 157 156
- min = 023 163 155 156 160 158 154 166 154 164 158
- min = 024 160 163 154 155 154 155 156 181 157 161
- min = 025 154 156 157 155 155 157 153 198 155 155
- min = 026 163 154 154 162 161 155 161 163 154 155
- min = 027 157 155 155 155 158 155 153 166 155 157
- min = 028 162 156 157 159 158 162 157 164 158 161
- min = 029 158 155 157 157 167 155 160 155 155 159
- min = 030 156 154 168 159 164 154 155 162 154 156
- min = 031 161 156 157 157 156 156 166 161 156 156
- min = 032 156 155 156 155 156 155 159 155 155 158
- min = 033 158 157 157 158 160 163 163 158 156 159
- min = 034 196 157 155 158 169 156 160 157 159 157
- min = 035 179 158 158 166 165 164 163 156 164 160
- min = 036 163 157 157 174 164 162 161 158 157 158
- min = 037 161 161 163 158 164 162 163 161 160 160
- min = 038 161 160 158 163 158 159 165 156 163 165
- min = 039 159 160 159 158 163 163 167 163 162 164
- min = 040 164 158 158 163 160 163 161 157 167 158
- min = 041 164 160 166 160 162 162 158 160 177 162
- min = 042 161 162 169 163 166 159 161 158 175 160
- min = 043 162 166 165 159 165 161 160 166 178 160
- min = 044 169 163 162 161 166 161 160 165 165 173
- min = 045 163 162 166 166 166 162 162 169 159 167
- min = 046 163 163 162 169 162 163 164 162 164 162
- min = 047 174 162 170 165 164 162 167 168 171 162
- min = 048 166 162 161 164 161 163 162 164 170 162
- min = 049 163 162 163 162 168 164 163 163 162 167
- min = 050 172 168 163 162 172 163 166 165 162 169
- min = 051 169 164 169 166 169 166 162 165 164 166
- min = 052 171 163 166 170 170 172 163 172 163 171
- min = 053 172 168 167 171 165 167 165 165 168 176
- min = 054 167 165 165 170 163 170 165 163 171 173
- min = 055 166 164 166 166 168 166 175 165 167 170
- min = 056 168 166 167 164 169 165 171 170 168 167
- min = 057 171 185 167 173 167 172 172 167 167 170
- min = 058 171 167 165 173 167 173 167 168 169 167
- min = 059 170 168 165 166 169 172 168 169 170 168
- min = 060 171 167 168 169 179 175 174 171 171 171
- min = 061 169 174 167 171 177 172 176 169 169 190
- min = 062 169 172 167 169 170 172 174 169 169 167
- min = 063 171 170 170 168 170 170 170 174 167 171
- min = 064 175 171 167 170 178 176 169 174 169 169
- min = 065 169 169 171 175 171 180 169 173 169 172
- min = 066 172 172 175 170 171 170 169 170 171 173
- min = 067 177 173 172 171 176 172 180 176 171 176
- min = 068 171 172 174 172 177 171 173 174 170 173
- min = 069 180 180 172 183 179 173 176 175 173 174
- min = 070 176 172 173 178 178 190 179 178 177 176
- min = 071 176 178 173 176 176 184 172 180 175 176
- min = 072 175 174 173 181 175 180 172 175 175 175
- min = 073 183 179 177 172 174 181 174 175 174 181
- min = 074 183 179 177 172 176 180 174 180 174 179
- min = 075 180 174 175 175 177 186 174 182 177 176
- min = 076 180 177 181 178 179 184 180 178 176 181
- min = 077 179 174 187 183 177 175 178 178 178 180
- min = 078 178 176 180 179 177 177 183 177 176 178
- min = 079 178 177 177 177 177 177 176 182 177 175
- min = 080 181 177 180 187 182 175 178 181 178 176
- min = 081 182 179 185 182 187 178 177 180 181 180
- min = 082 180 179 179 179 184 178 178 181 182 178
- min = 083 180 178 180 177 186 178 178 184 183 184
- min = 084 178 188 180 185 186 180 182 181 181 187
- min = 085 179 187 179 179 182 179 180 181 179 184
- min = 086 180 186 180 185 188 184 182 180 180 181
- min = 087 183 181 180 181 188 190 180 185 181 183
- min = 088 186 189 189 182 184 190 187 181 186 182
- min = 089 185 191 192 184 183 185 184 185 183 184
- min = 090 183 183 184 185 182 186 181 183 184 183
- min = 091 193 188 183 184 184 182 184 183 188 189
- min = 092 190 184 184 187 187 185 184 183 184 187
- min = 093 192 191 186 187 192 185 184 187 192 211
- min = 094 184 187 186 210 191 188 186 184 189 186
- min = 095 185 193 186 194 192 195 184 186 184 186
- min = 096 187 186 185 184 187 186 188 186 185 189
- min = 097 193 186 194 198 188 191 186 189 190 188
- min = 098 190 187 191 192 188 194 188 190 188 191
- min = 099 187 188 187 189 201 186 188 188 191 189
- min = 100 190 193 189 192 200 196 192 192 187 188
- min = 101 193 188 188 195 199 195 192 192 187 191
- min = 102 192 187 188 194 202 188 193 190 188 190
- min = 103 194 188 188 194 198 188 204 194 195 191
- min = 104 190 190 189 193 194 191 191 193 192 191
- min = 105 198 197 194 196 194 193 192 192 192 191
- min = 106 193 189 194 191 191 192 191 191 193 197
- min = 107 192 191 192 196 194 192 193 191 193 198
- min = 108 194 199 195 196 196 193 192 196 195 195
- min = 109 193 197 194 194 193 193 197 196 197 193
- min = 110 196 217 195 197 195 194 207 202 198 196
- min = 111 199 197 194 201 195 202 195 194 202 196
- min = 112 202 193 198 194 195 197 196 195 197 196
- min = 113 196 194 198 203 195 196 194 196 201 196
- min = 114 196 198 196 201 198 195 196 201 195 197
- min = 115 197 201 199 199 204 197 194 199 199 201
- min = 116 197 197 196 198 197 203 200 201 195 200
- min = 117 198 199 202 201 199 200 196 196 199 196
- min = 118 198 200 205 197 200 198 229 197 198 206
- min = 119 199 201 200 197 205 197 247 199 199 199
- min = 120 198 201 200 199 205 199 232 201 200 209
- min = 121 203 198 206 199 201 199 219 204 201 213
- min = 122 201 199 208 199 198 200 211 202 198 215
- min = 123 207 200 203 201 207 205 204 202 206 216
- min = 124 203 204 213 204 209 203 201 230 207 204
- min = 125 205 202 202 203 211 202 201 208 216 205
- min = 126 210 206 202 217 217 207 203 214 203 205
- min = 127 222 201 201 201 205 202 205 206 201 203
- min = 128 214 201 204 203 211 202 205 211 207 204


# Algorithms
The algorithms have features from both BST and B tree.

## Search
```
start from root node
if left child is not null and search key is less than the least key then:
	Search the left child recursively
	done
end if
if right child is not null and search key is greater than the greatest key then:
	Search the right child recursively
	done
end if
binary search in the current node
return the result
```

## Insertion
```
start from root node
if left child is not null and search key is less than the least key then:
	Insertion at the left child recursively
	fix double red violation
	done
end if
if right child is not null and search key is greater than the greatest key then:
	Insertion at the right child recursively
	fix right red violation
	done
end if
binary search in the current node
if found then:
	update value
	done
end if
if node is not full then:
	insert at correct index
	done
end if
create a new node
move half of the existing key/value pais from current node to the new node and insert the new key/value pair
insert the new node as the least of the right subtree
fix right red violation and double red violation
```

## Deletion
```
start from root node
if left child is not null and search key is less than the least key then:
	Deletion at the left child recursively
	handle deletion at leaf
	resolve double black
	fix double black
	done
end if
if right child is not null and search key is greater than the greatest key then:
	Deletion at the right child recursively
	handle deletion at leaf
	resolve double black
	fix double black
	done
end if
binary search in the current node
if not found then:
	do nothing on non-existing key
	done
end if
delete at the correct index
if elements of current node is not less than the minimum
	done
end if
if the node is leaf then:
	label to let the parent to handle deletion at leaf
	done
end if
if the node has only one child then:
	merge or borrow from left child // for left leaning red black tree, the only possibility is a red leaf at left
	done
end if
if the node has two children:
	borrow or merge with its successor (minimum of right sub tree)
	resolve double black
	done
end if
```

## Traversal
Same as traversal of BST. When visiting each node, visit each element in key array and value array.
