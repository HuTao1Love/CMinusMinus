# CMinusMinus

Example programming language with its own bytecode, VM with GC and ~Jit.

## Made by
- Захар Ягодин
- Александр Иоффе
- Артём Марченко
- Артём Артемьев

## How to run
- You can find examples of code in src/Program/examples directory
- You can run it only to compile source code
    > program compile <SourceFile>
- Run program from bytecode
    > program run <SourceFile>
- Or compile & run it
    > program <SourceFile>

## Arguments & options:
- SourceFile - file to compile & run
- -i - write more information about compile & run of program
- -o - enable optimizations
- -h - watch help and usage information

## How it works

### Project Compiler
- ANTLR for parsing to CST
- Generate AST from CST (Capsaicin.VisitorPatternGenerator for generation visitor pattern for my AST)
- CmmCompilerVisitor.cs for generate .cmmbin file with CmmObjectVisitor

### Project Interpreter
- VirtualMachine class that reads all bytecode from .cmmbin file and execute it
- GarbageCollector (mark&sweep) runned from VM
- Optimizer directory with JIT optimizers
