namespace Interpreter;

// ReSharper disable InconsistentNaming
public enum VmInstructionType
{
    Push,
    Pop,
    Print,
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    CompLT,
    CompGT,
    CompGE,
    CompLE,
    CompNE,
    CompEQ,
    Jz,
    Jmp,
    Neg,
    Call,
    Return,
    Array,
    Access,
    Length,
    BinAnd,
    BinOr
}
