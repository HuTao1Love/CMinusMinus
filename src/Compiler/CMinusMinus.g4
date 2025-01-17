grammar CMinusMinus;

// Лексер
WS      : [ \t\r\n]+ -> skip ;
INT     : [0-9]+ ;
ID      : [a-zA-Z_][a-zA-Z_0-9]* ;

// Ключевые слова
WHILE   : 'while';
IF      : 'if';
ELSE    : 'else';
PRINT   : 'print';
RETURN  : 'return';

// Операторы
EQ      : '==';
GE      : '>=';
LE      : '<=';
ASSIGN  : '=';
PLUS    : '+';
MINUS   : '-';
MUL     : '*';
DIV     : '/';
LT      : '<';
GT      : '>';

// Символы
LPAREN  : '(';
RPAREN  : ')';
LBRACE  : '{';
RBRACE  : '}';
COMMA   : ',';
SEMICOLON : ';';

// Парсер
program
    : stmt* EOF
    ;

stmt
    : expr SEMICOLON
    | block
    | ifStmt
    | whileStmt
    | printStmt
    | returnStmt
    ;

block
    : LBRACE stmt* RBRACE
    ;

ifStmt
    : IF LPAREN expr RPAREN stmt (ELSE stmt)?
    ;

whileStmt
    : WHILE LPAREN expr RPAREN stmt
    ;

printStmt
    : PRINT LPAREN expr RPAREN SEMICOLON
    ;

returnStmt
    : RETURN expr SEMICOLON
    ;

expr
    : expr (PLUS | MINUS) expr
    | expr (MUL | DIV) expr
    | expr (EQ | GE | LE | LT | GT) expr
    | ID ASSIGN expr
    | INT
    | ID
    | LPAREN expr RPAREN
    ;
