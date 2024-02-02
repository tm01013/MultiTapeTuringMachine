# MTTM Syntax guide

### Tabe of contents

1. [Basic outline](#basic-outline)
2. [Base structure](#base-structure)
3. **[Moving the pointer](#moving-the-pointer)**
4. **[Modifying cell values](#modifying-cell-values)**
5. **[IO](#input-output)**
6. **[Flow contrlol](#flow-control)**
7. **[Separate sections](#separate-sections)**
8. **[Owerflow](#owerflow)**
9. **[Saving tapes](#saveing-and-loading-tapes)**
10. [Advanced commands](#advanced-commands)

<br>

### `WARNING you will read a relly boaring and detaild docs below!!`

---

## Basic outline

- MTTM **instrucions** operate on one of it's 13 tapes
  - Different instructions can operate on different tapes
  - Tapes are named the following: T1 ... T13
- Every tape contains 1001 cells
- Every cell can contain a intager between 0 and 255
  - Every cell by default is initialised to 0
- Every tape has it's own pointer (only the cell where is the pointer currently is can be accesed)

---

## Base structure

- **Lines separated by linebrakes or semicolons(;)**
- The most MTTM program line will look like this:
  ```
  T1> GO.UNTIL $STATUS
  [tape witch the command will oparate on] '>' [The actual command] [arguments]
  ```
- Not all commands require a tape to operate on, in that case you simply don't give the tape.

  ### Command structure

  - Every command have a **_opcode_** witch defines the command (eg. GO, BACK, ADD)
  - Some of the commands have/can have **_secondary opcodes_** witch defines a more specific command.
  - _secondary opcodes_ are given like this:
    ```
    opcode
    V
    GO.UNTIL
        ^
        secondary opcode
    ```

  ### Arguments

  - Arguments are separeted with spaces
  - A argument can be a intager between 0 and 255 or a variable
  - Variables can be called with a dollar sign($)
  - The following variables exist:
    - Oter tape's value (at it's pointer's location). ==> `$<tape name>` (eg. `$T4`)
    - The machines _status_ variable (read more about later) ==> `$STATUS`

  ### Comments

  - Comments can placed anywhere (except in the middle of a line)
  - Comments starts with `// `
    > The space after the fordslashes are important !

---

## Moving the pointer

### 1. `GO`

- It moves the pointer forward/right
- Requires a tape to operate on
- Doesn't accept any arguments
- Can be stacked (two or more `GO` commands can put in a single line without starting a new line)
- Optionally can used with the `UNTIL` secondary opcode
  #### `GO.UNTIL`
  - It moves the pointer forward/right until it reaches a cell with the value given as a argument.
    > If it don't find any cell with the value given as a argument it won't move
  - Requires a tape to operate on
  - Requires one argument
  - Can't stacked

### 2. `BACK`

- It moves the pointer backward/left
- Requires a tape to operate on
- Doesn't accept any arguments
- Can be stacked (two or more `BACK` commands can put in a single line without starting a new line)
- Optionally can used with the `UNTIL` secondary opcode
  #### `BACK.UNTIL`
  - It moves the pointer backward/left until it reaches a cell with the value given as a argument.
    > If it don't find any cell with the value given as a argument it won't move
  - Requires a tape to operate on
  - Requires one argument
  - Can't stacked

### 3. `JUMP`

- Must used with the `GO` or `BACK` secondary opcode
- Requires a tape to operate on
- Doesn't accept any arguments
- I don't know any practical usecase for this command :)
  #### `JUMP.GO`
  - It moves the pointer forward/right as many times as the starting cell's value
  #### `JUMP.BACK`
  - It moves the pointer backword/left as many times as the starting cell's value

### 4. `PREV`

- Requires a tape to operate on
- Doesn't accept any arguments
- It will undo the last pointer movement command, on the specified tape.

---

## Modifying cell values

### 1. `INC` and `DEC`

- Require a tape to operate on
- Don't accept any arguments
- Cannot used with a secondary opcode
- Can stacked
  #### `INC`
  - This will increment the cells value by 1
  - If it reach 255 it will overflow to 0 depending on the owerflow status
    > More about it in the [owerflow section](#owerflow)
  #### `DEC`
  - This will decrement the cells value by 1
  - If it reach -1 it will overflow to 255 depending on the owerflow status
    > More about it in the [owerflow section](#owerflow)

### 2. `ADD`, `SUB`, `MUL`, `DIV`, `MOD`

- Require a tape to operate on
- They require one arguments
- They run operations on the current cell and the result will be saved also to the current cell.
- **The cells value is always on the left side of the operator!!**
  > ADD => addition <br>
  > SUB => substraction <br>
  > MUL => multiplication <br>
  > DIV => division (the result will be cast to a int) <br>
  > MOD => mudulo operator (%) <br>
- They accept the `GO` and the `BACK` secondary opcode

  #### With `GO` or `BACK` secondary opcode

  - **They don't accept any arguments**
  - **The current cells value is always on the left side of the operator!!**
  - The left side of the operator will be the **_next (`GO`) or the previous (`BACK`) cells value_**

### 3. `RND`

- Require a tape to operate on
- Doesn't accept any arguments
- Cannot used with a secondary opcode
- **It will put a random number between 0 and 255 to the current cell**

### 4. `RESET`

- Require a tape to operate on
- Doesn't accept any arguments
- Cannot used with a secondary opcode
- **It will set the current cells value to 0**

### 5. `CP`

- Require a tape to operate on
- Doesn't accept any arguments
- _Must used with the `GO` or `BACK` secondary opcode_
- **It will copy the current cells value to the next (`GO`) or to the previous (`BACK`)**

---

## Input, Output

### 1. `IN`

- Require a tape to operate on
- Doesn't accept any arguments
- Cannot used with a secondary opcode
- **It will promot the user for input**
- As input the program accepts a single ascii letter -> the ascii value will be put to the current cell
- Also accepts intagers as input -> the intager will be put to the current cell
  > If the number is bigger than 255 it will [owerflow](#owerflow) if its on

### 2. `OUT`

- Require a tape to operate on
- Doesn't accept any arguments
- Optionally can used with the `ASCII` secondary opcode
- It will output the current cells value as a intager
- If used with the `ASCII` secondary opcode it will output the current cells value as a ascii character

---

## Flow control

### 1. Labels

- Labels can defined by its name then a comma (:)
  > The labels name only can contain letters and digits! <br>
  > Example:
  >
  > ```
  > someLabel123:
  > ```

### 2. `IF` statements

- Require a tape to operate on
- Requires **two** arguments
- Before using it read the "_[Separate secions](#separate-sections)_"!
- **The current cells value is on the left side of the operator, the first argument will be placed to the right side**
- **The second argument is the name of the label where to jump to when the statement is true**
- Must used with a secondary opcode:
  > `IF.IS` equal <br> > `IF.NOT` not equal <br> > `IF.LESS` if smaller <br> > `IF.BIGGER` if bigger <br> <br>
  > Example: <br>
  >
  > ```
  > T4> IF.NOT 33 someLabel
  > ```

### 3. `GOTO`

- Doesn't require a tape to operate on
- Requires one argument
- Cannot used with a secondary opcode
- **It will jump to the specified label**

### 4. `HALT`

- Doesn't require a tape to operate on
- Doesn't accept any argument
- Cannot used with a secondary opcode
- **This will terminate the program**

---

## Separate sections

- **Code inside a separate section will be ignored!**
- **The _only way to execute code inside_ a separate section is by _using labels_** <br><br>
- A separate section can opened using the `SS.START` command
- A separate section can be ended using the `SS.END` command
  > These commands don't require a tape to operate on and also don't require any arguments
- **All opened separate section must be closed!**
- You can put a separate section inside a separate section

---

## Owerflow

- By default owerflow is on
- Owerflow can turned on with the `OWERFLOW ON` command
- Owerflow can turned off with the `OWERFLOW OFF` command

  > These commands don't require a tape to operate on and cannot used with a secondary opcode

  ### When ON

  - Every time you try to go above or below the max or min cell value the operation will be countinued from the opposite end
    > Example: 255 + 1 = 0; 0 - 1 = 255; 255 + 2 = 1 etc...
  - Every time you try to go further then the last cell the operation will countinue on the begining of the tape
  - Every time you try to go backer (?) then the first cell the operation will countinue on the end of the tape

  ### When OFF

  - Every operaion that try to exceed the min or max limit the part of the operation that will go outside the bounds will ignored

---

## Saveing and Loading tapes

- Tapes can saved or loaded from a MTTM tapefile (.tmt)

  ### `SAVE`

  - It requires a tape to oparate on (this will be saved)
  - Cannot used with a secondary opcode
  - Requires one argumet
  - As argument it requires the output file name with path
    > The path can relative to the program that is currently executing or absolute
  - **Files with the same name will be owerritten without warning!**

  ### `LOAD`

  - It requires a tape to oparate on (the tape where the tape will be loaded)
  - Cannot used with a secondary opcode
  - Requires one argumet
  - As argument it requires the input file name with path
    > The path can relative to the program that is currently executing or absolute

---

## Advanced commands

- `WARNING this commands aren't useful at all!`
- I have no idea why i put them in the language..

### 1. `CUT`

- It requires a tape to operate on
- Doesn't accept any argument
- Cannot used with a secondary opcode
- **It will cut the specified tape after the pointers location** -> The cells after that cannot be accesed any more
  > But these cells are present in saved tapes
