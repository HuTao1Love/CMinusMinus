grammar CMinusMinus;

// lexer
ASSIGN       : '=';
EQUAL        : '==';
PLUS         : '+';
MINUS        : '-';
MULT         : '*';
DIVIDE       : '/';
LT           : '<';
GT           : '>';
POPEN        : '(';
PCLOSE       : ')';
BROPEN       : '{';
BRCLOSE      : '}';
BOPEN        : '[';
BCLOSE       : ']';
COMMA        : ',';
SEMICOLON    : ';';
ID           : [a-zA-Z_][a-zA-Z0-9_]*;
INT          : [0-9]+;
WHITESPACE   : [ \t\r\n]+ -> skip;
COMMENT      : '//' ~[\r\n]* -> skip;

// parser
program: function_declaration+ EOF;

function_declaration
    : 'function' name=ID '(' (args=ID (',' args=ID)*)? ')' block
    ;

statement
    : block                                    # statement_block
    | if                                       # statement_if
    | for                                      # statement_for
    | while                                    # statement_while
    | function                                 # statement_function
    | assignment ';'                           # statement_assignment
    | return                                   # statement_return
    | print                                    # statement_print
    ;

if
    : 'if' '(' expression ')' ifblock=block ('else' elseblock=block)?
    ;

for
    : 'for' '(' assignment ';' expression ';' assignment ')' block
    ;

while
    : 'while' '(' expression ')' block
    ;

function
    : ID '(' expression? (',' expression)* ')'
    ;

assignment
    : var '=' expression                   # assignment_value
    | 'array' ID '[' expression ']'        # assignment_array
    ;

var
    : ID                           # variable
    | ID ('[' expression ']')+     # array
    ;

return
    : 'return' expression? ';'
    ;

print
    : 'print' expression? ';'
    ;

expression
    : var                                                                           # expression_variable
    | function                                                                      # expression_function
    | value                                                                         # expression_value
    | '(' expression ')'                                                            # expression_brackets
    | operator=('-' | '!') expression                                               # expression_negation
    | expression operator=('/' | '*') expression                                    # expression_calc
    | expression operator=('+' | '-') expression                                    # expression_calc
    | expression operator=('==' | '!=' | '<' | '>' | '<=' | '>=') expression        # expression_logical
    | expression operator='&&' expression                                           # expression_logical
    | expression operator='||' expression                                           # expression_logical
    ;

block
    : BROPEN statement* BRCLOSE
    ;

value
    : INT
    ;
