grammar Cmm;

program
    : function* EOF
    ;

function
    : function_return_type name=ID '(' ID? (',' ID)* ')' block
    ;
    
function_return_type
    : type               # function_return_some_type
    | 'void'             # function_return_void
    ;
    
function_call
    : ID '(' expression? (',' expression)* ')'
    ;
 
block
    : '{' statement* '}'
    ;
    
statement
    : block                                    # statement_block
    | if                                       # statement_if
    | function_call                            # statement_function
    | assignment                               # statement_assignment
    | assignment_with_initialization           # statement_assignment_init
    | initialization                           # statement_init
    | return                                   # statement_return
    ;

if
    : 'if' '(' expression ')' ifblock=block ('else' elseblock=block)?
    ;
    
assignment
    : ID '=' expression
    ;
    
assignment_with_initialization
    : type ID '=' expression
    ;
    
initialization
    : type ID
    ;
    
return
    : 'return' expression?
    ;
    
expression
    : ID                                                                   # expression_variable
    | function_call                                                        # expression_function
    | value                                                                # expression_value
    | '(' expression ')'                                                   # expression_brackets
    | ('-' | '!') expression                                               # expression_negation
    | expression '*' expression                                            # expression_calc
    | expression ('+' | '-') expression                                    # expression_calc
    | expression ('==' | '!=' | '<' | '>' | '<=' | '>=') expression        # expression_logical
    | expression '&&' expression                                           # expression_logical
    | expression '||' expression                                           # expression_logical
    ;

type
    : 'number'
    | 'string'
    | 'bool'
    ;
    
value
    : number | string | bool
    ;
    
number  
    : NUMBER
    ;
    
string
    : STRING
    ;

bool
    : BOOL
    ;

ID
    : [a-zA-Z_][a-zA-Z_0-9]*
    ;
    
NUMBER
    : [0-9]+
    ;
    
STRING
    : '"' .*? '"'
    ;
    
BOOL
    : [Tt][Rr][Uu][Ee] | [Ff][Aa][Ll][Ss][Ee]
    ;

WS
    : [ \t\r\n]+ -> skip
    ;

COMMENT
    : '//' ~[\r\n]* -> skip
    ;