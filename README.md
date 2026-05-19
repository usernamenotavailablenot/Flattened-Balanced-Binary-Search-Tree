# Balanced-Binary-BTree
This is an implementation of Balanced Binary B Tree (B3 Tree), a hybrid of balanced binary search tree and B tree supporting fast search, insertion, and deletion. The idea is from B(+) tree. The lowest level of B(+) tree is a sorted linked list, and a sorted linked list is equivalent to a binary search tree (BST). Then, the lowest level of B(+) tree can be converted to a BST, and the upper levels, which serve as indexes can be discarded. The BST can be balanced by techniques like AVL tree or Red/Black tree. In this implementation, Left Leaning Red Black tree is adopted.

The B3 has following features from BST:
- Each node has exactly two children; 
- Balancing technique is the same.

The B3 has following features from B Tree: 
- Each node stores a collection of elements; 
- Collection has a lower and upper limit; 
- Insertion and deletion may involve node split and merge.

The advantages of B3: 
- more space efficient, since B3 requires much fewer pointers.
- more cache friendly, since B3 stores data in arrays.

The run time complexity is obviously `Log(N)` for search, insertion, and deletion. For benchmarking, the attached unit test instantiates 128 B3 trees with different node capacity. Each B3 is then performed with 100,000 random insertions and 100,000 random deletions. The whole test took around 1 minute in my laptop with Intel(R) Core(TM) i7-3630QM CPU @ 2.40GHz and 8.00 GB Installed DDR3 RAM, which was purchased before 2014, with each B3 tree spending 300-600 milliseconds. The attached unit test code runs the test above 10 times, and below is a sample output.

- min = 000 804 533 525 527 585 524 538 510 513 576
- min = 001 559 515 481 476 482 553 486 488 536 572
- min = 002 430 402 446 440 441 433 450 467 429 468
- min = 003 388 369 370 410 382 358 376 373 363 432
- min = 004 379 365 361 370 378 357 366 376 339 385
- min = 005 347 354 343 363 360 368 362 370 362 381
- min = 006 339 348 325 379 329 329 362 366 376 344
- min = 007 347 350 343 338 356 356 333 335 326 336
- min = 008 328 376 345 333 351 338 353 348 355 331
- min = 009 334 351 320 339 334 334 316 328 323 318
- min = 010 319 344 358 345 346 320 324 342 334 343
- min = 011 316 328 334 317 325 333 321 326 331 330
- min = 012 319 381 346 322 322 340 315 344 324 331
- min = 013 314 378 332 325 329 322 351 318 326 340
- min = 014 349 521 314 313 351 336 331 334 323 346
- min = 015 399 498 341 355 323 338 321 319 331 330
- min = 016 386 370 313 311 341 307 307 329 329 362
- min = 017 315 312 320 333 317 314 329 322 329 321
- min = 018 348 330 308 323 323 342 347 309 360 322
- min = 019 339 336 312 313 331 328 311 321 323 309
- min = 020 309 319 326 333 336 323 328 315 323 322
- min = 021 321 310 335 319 326 331 350 313 313 335
- min = 022 329 316 313 330 316 333 337 339 309 319
- min = 023 317 321 322 331 314 347 343 313 319 342
- min = 024 334 314 310 309 343 336 329 321 316 325
- min = 025 316 332 312 326 363 391 338 331 319 321
- min = 026 324 317 310 321 356 325 377 320 318 325
- min = 027 334 332 325 329 315 329 333 314 317 324
- min = 028 326 349 318 335 332 330 337 330 350 343
- min = 029 332 323 356 343 343 338 351 336 321 357
- min = 030 329 326 331 313 333 336 347 327 326 371
- min = 031 319 336 339 334 322 343 342 343 323 325
- min = 032 328 326 330 325 325 340 349 318 317 327
- min = 033 332 338 320 323 342 325 340 320 322 319
- min = 034 329 324 329 358 328 336 351 324 328 341
- min = 035 323 332 326 346 341 331 331 327 329 334
- min = 036 326 321 347 323 355 335 335 327 325 332
- min = 037 348 325 320 336 345 322 317 337 353 342
- min = 038 341 334 336 326 335 332 351 332 327 329
- min = 039 325 326 336 341 340 328 323 324 332 332
- min = 040 325 336 327 337 333 333 330 344 336 338
- min = 041 358 355 350 322 326 325 335 333 330 332
- min = 042 354 328 335 343 340 332 339 348 348 329
- min = 043 335 331 343 340 332 338 328 329 333 363
- min = 044 357 354 364 341 333 342 335 357 328 350
- min = 045 338 332 351 329 353 326 347 346 358 355
- min = 046 336 335 342 333 345 333 338 336 334 350
- min = 047 341 336 347 341 334 342 330 344 341 332
- min = 048 357 349 341 348 352 347 334 360 332 359
- min = 049 357 334 354 368 351 345 336 357 364 333
- min = 050 346 361 333 341 351 337 345 346 348 359
- min = 051 345 350 348 344 344 336 335 347 357 366
- min = 052 353 353 348 335 341 351 363 335 343 359
- min = 053 355 344 360 337 341 385 339 338 343 361
- min = 054 346 344 355 345 335 352 341 346 363 344
- min = 055 343 363 364 349 355 339 336 338 358 358
- min = 056 355 352 368 367 360 354 356 349 348 344
- min = 057 357 342 359 366 344 341 344 347 341 413
- min = 058 354 361 361 358 350 345 342 349 351 383
- min = 059 373 342 348 344 342 367 353 343 347 373
- min = 060 389 347 355 358 348 361 346 356 360 376
- min = 061 380 350 356 364 350 344 376 362 354 372
- min = 062 367 373 350 349 352 371 369 349 357 389
- min = 063 347 349 355 347 361 356 343 347 357 356
- min = 064 358 365 350 356 352 376 345 351 384 384
- min = 065 400 353 350 362 382 361 353 371 385 361
- min = 066 379 360 366 351 393 372 350 376 361 379
- min = 067 369 350 362 373 363 354 362 354 384 362
- min = 068 402 354 357 365 391 356 369 354 352 364
- min = 069 435 358 361 351 368 352 353 354 371 378
- min = 070 379 375 398 352 361 368 368 364 355 404
- min = 071 378 366 364 365 360 357 355 368 358 395
- min = 072 370 373 497 358 357 377 359 353 371 364
- min = 073 382 358 403 351 359 364 393 364 375 378
- min = 074 383 356 360 367 370 372 355 356 372 378
- min = 075 372 373 370 371 378 375 375 366 378 358
- min = 076 366 376 367 378 370 357 357 360 359 403
- min = 077 368 371 394 377 368 398 356 358 361 359
- min = 078 371 393 372 380 360 404 365 359 371 362
- min = 079 381 362 382 376 381 361 375 367 400 365
- min = 080 384 390 377 376 376 397 371 383 382 388
- min = 081 374 368 375 369 379 405 374 369 375 369
- min = 082 398 382 372 376 370 375 369 373 368 391
- min = 083 388 378 398 385 380 386 373 387 380 366
- min = 084 374 378 374 389 389 387 366 384 382 401
- min = 085 458 372 392 374 376 367 374 381 380 369
- min = 086 394 375 377 365 383 366 374 394 374 380
- min = 087 404 400 385 379 384 381 386 384 407 397
- min = 088 394 374 373 379 378 395 373 378 385 379
- min = 089 393 393 404 386 389 402 397 377 388 379
- min = 090 393 393 404 411 378 386 397 386 389 427
- min = 091 388 410 403 393 387 391 405 407 385 477
- min = 092 378 407 402 396 380 386 393 381 383 412
- min = 093 388 391 389 387 384 382 392 388 387 420
- min = 094 398 391 382 381 391 412 386 407 379 408
- min = 095 387 387 390 390 405 409 391 384 398 412
- min = 096 394 394 404 388 403 435 386 381 395 429
- min = 097 397 391 395 391 396 425 384 392 383 422
- min = 098 435 409 384 407 399 449 396 389 384 434
- min = 099 416 444 420 397 401 416 422 408 417 396
- min = 100 400 496 399 404 414 428 413 397 407 410
- min = 101 389 410 395 396 388 434 409 403 423 394
- min = 102 398 390 404 393 394 433 410 391 430 393
- min = 103 402 453 396 410 396 433 413 402 411 417
- min = 104 397 400 392 420 406 437 431 408 404 407
- min = 105 408 410 398 395 393 408 419 392 397 392
- min = 106 408 399 392 415 407 422 427 405 422 417
- min = 107 399 399 405 402 406 399 414 408 415 423
- min = 108 404 410 424 411 399 406 417 413 421 406
- min = 109 399 408 416 404 431 397 477 408 400 422
- min = 110 426 417 431 408 401 429 465 409 404 401
- min = 111 414 416 410 402 424 409 402 438 405 426
- min = 112 402 400 416 413 414 408 495 414 410 413
- min = 113 423 404 404 421 421 417 446 429 417 433
- min = 114 410 413 430 418 401 410 410 418 404 411
- min = 115 407 410 403 415 407 414 415 466 403 423
- min = 116 410 417 413 410 429 412 417 407 417 425
- min = 117 412 412 425 407 415 432 408 426 427 415
- min = 118 426 406 423 420 435 415 419 412 422 425
- min = 119 422 438 424 437 447 431 414 424 433 432
- min = 120 419 423 430 417 436 433 412 431 466 463
- min = 121 430 423 449 423 491 430 409 435 535 460
- min = 122 419 416 414 420 478 429 415 418 537 427
- min = 123 424 422 439 425 474 422 435 422 453 443
- min = 124 452 461 426 419 440 416 448 442 469 454
- min = 125 417 435 435 432 434 421 438 444 430 416
- min = 126 440 439 429 422 429 412 423 428 442 417
- min = 127 420 428 424 418 419 428 419 449 431 436
- min = 128 415 456 436 423 457 435 431 424 434 444

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
	merge or borrow from left child // for left leaning red black tree, the only possiblity is a red leaf at left
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
