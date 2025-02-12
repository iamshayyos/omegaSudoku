# Omega Sudoku Solver

## Overview
The Omega Sudoku Solver is a **C#-based** Sudoku-solving application designed to handle **N×N Sudoku boards** with sizes ranging from **1×1 to 25×25**. The goal of this project is to provide an efficient, scalable, and modular approach to solving Sudoku puzzles using advanced algorithms and heuristics.

## Sudoku Rules
Sudoku is played on an **N×N grid**, with the following constraints:
- Each row, column, and subgrid (of size **√N × √N**) must contain **all numbers from 1 to N** without repetition.
- Empty cells are represented by **0**.
- The maximum board size supported is **25×25**, and N must be a **perfect square**.

## Features and Requirements
1. **Support for variable board sizes**: The solver can handle Sudoku boards from **1×1 up to 25×25**.
2. **Multiple input methods**: Users can input Sudoku puzzles via **console** or **text files**.
3. **Optimized solving techniques**:
   - **Bitwise backtracking algorithm**
   - **Human-solving heuristics** (Hidden Singles, Naked Singles, Naked Pairs, etc.)
   - **Memory-efficient board management**
4. **Console-based UI**: The solution and board status are displayed in a structured format.
5. **Fast performance**: **16×16 and smaller Sudoku boards solved in under 1 second** 

## About the Project
This project is structured as a **generic board game solver** implemented in **C#**, optimized for Sudoku. The codebase follows **object-oriented programming (OOP) principles**, **multiple interfaces**, and **SOLID design patterns**, making it straightforward to extend functionality for other board-based games.

### Solving Methods
The solver employs a hybrid approach combining **bitwise backtracking** and **heuristic-based solving**:
1. **Apply human tactics** to simplify the board.
2. **Identify the empty cell with the fewest possible values (MRV heuristic).**
3. **Use backtracking** to test values recursively.
4. **If a valid solution is found, return true.**
5. **If the board is not solvable, revert changes and try the next candidate.**
6. **If no candidates remain, return false.**

## About the heuristics
1. Naked Singles

Concept:
For every empty cell, the solver computes the list of candidate numbers that are valid based on the current row, column, and subgrid constraints.

How It Works:
If a cell’s candidate list contains only one number, that number must be the correct choice. The solver then fills in that value immediately.

Impact:
This direct assignment not only solves the cell but also propagates restrictions to neighboring cells, thereby narrowing down their candidate lists.

2. Hidden Singles

Concept:
Occasionally, a candidate may not be unique within its own cell but is the only instance that can appear in a particular row, column, or subgrid.

How It Works:
The solver examines each unit (row, column, or subgrid) to check whether any number appears as a candidate in only one cell within that unit. If found, that number is placed in the corresponding cell.

Impact:
By revealing hidden necessities that aren’t immediately obvious from individual cell candidates, this method uncovers moves that further constrain the puzzle.

3. Naked Pairs

Concept:
In some units, two cells might share an identical pair of candidates and no other options.

How It Works:
When such a pair is detected, the solver deduces that these two numbers must occupy these two cells in some order. As a result, it eliminates these two candidates from the candidate lists of all other cells in the same unit.

Impact:
This elimination sharpens the focus on the remaining cells, often leading to additional placements through either naked or hidden singles.

4. Minimum Remaining Values (MRV) Heuristic

Concept:
When the puzzle can no longer be simplified solely by the above methods, the MRV heuristic guides the next move.

How It Works:
The algorithm scans the board to identify the empty cell with the fewest legal candidates. By choosing this most-constrained cell, the number of subsequent possibilities—and thus the potential backtracking—is minimized.

Impact:
Prioritizing the cell with minimal options greatly reduces the overall search space, leading to faster resolution during the backtracking phase.

### Bitwise Optimization
To improve performance, the solver utilizes **bitwise operations**:
- **Tracks row, column, and subgrid constraints** using integer bitmasks.
- **Bitwise operations allow for fast lookups and updates**.
- **Validation checks** instead of iterating through rows, columns, or subgrids.

Example:
- A row stored as `100110111` indicates that values **4, 7, and 8** are missing.
- Using bitwise operations, constraints can be updated efficiently.

### Human Solving Heuristics
The solver incorporates three primary human-based techniques:
1. **Naked Singles** – If a cell has only one possible value, assign it.
2. **Hidden Singles** – If a number appears only once in a row, column, or subgrid, assign it.
3. **Naked Pairs** – If two cells contain the same two candidates, remove those values from the rest of the unit.

These heuristics speed up solving and enable the algorithm to solve some complex boards more efficiently than pure backtracking.

## User Interface & Input Methods
Upon launching the program, users can :
1. **Solve a Sudoku board (manual or from a file)**.
2. **Exit the program**.

### Input Options
- **Console Input**: Users manually enter a Sudoku board as a string.
- **File Input**: Users provide a text file containing a Sudoku board; the solution is saved in a new file.

### Example Execution
```
Welcome to Omega Sudoku!
Choose an option:
1) Enter Sudoku puzzle manually
2) Read Sudoku puzzle from a file
Or type 'end' to exit.
> 1
Enter the Sudoku puzzle as a single string:
> 530070000600195000098000060800060003400803001700020006060000280000419005000080079

Solved Sudoku board:
+-------+-------+-------+
| 5 3 4 | 6 7 8 | 9 1 2 |
| 6 7 2 | 1 9 5 | 3 4 8 |
| 1 9 8 | 3 4 2 | 5 6 7 |
+-------+-------+-------+
| 8 5 9 | 7 6 1 | 4 2 3 |
| 4 2 6 | 8 5 3 | 7 9 1 |
| 7 1 3 | 9 2 4 | 8 5 6 |
+-------+-------+-------+
| 9 6 1 | 5 3 7 | 2 8 4 |
| 2 8 7 | 4 1 9 | 6 3 5 |
| 3 4 5 | 2 8 6 | 1 7 9 |
+-------+-------+-------+
```

## Installation & Running the Project
### Prerequisites
- .NET 6.0 or later
- Visual Studio 2019/2022

### Setup Instructions
```bash
git clone https://github.com/iamshayyos/omegaSudoku.git
cd omegaSudoku
dotnet run
```


## Code Structure
```
├── omegaSudoku
│   ├── Interfaces           # Interfaces for modularity (ISudokuSolver, IValidator)
│   ├── CoreLogic            # Core solving logic and validation
│   ├── Board                # Sudoku board representation
│   ├── IO                   # Input handling (console & file)
│   ├── Heuristics           # Optimization techniques (Hidden Singles, Naked Pairs, etc.)
│   ├── Exceptions           # Custom exceptions
│   ├── Program.cs           # Entry point of the application
├── SudokuTests
│   ├── SolverTests          # Test for the Solver functionality
│   ├── ValidatorTests       # Test for the Validator functionality

```

## Testing
The project includes **unit tests** for validating board input and solving various Sudoku puzzles.
To run tests:
```bash
dotnet test
```
```InViaualStudio
right click on the SudokuTests file, then press run tests
```



