grammar ModelGrammar;

/*
 * Parser Rules
 */

number		: value=(FLOAT|INT);
complexExp	: left=number type=(IM| ANGLE) right=number;
complex		: IM im=number;
constant	: number  #NumberConstant
	| complex #ComplexConstant
	| complexExp #ComplexExprConstant
	| value=STRING #StringConstant
;

model: statement model | EOF;
statement: expression SEMICOLON | SEMICOLON;

binaryOperator: op=(PLUS | MINUS | ASSIGN | ASTERICS | DIVISION);
unaryOperator: op=(PLUS | MINUS);

expression: left=expression op=binaryOperator right=expression	#BinaryOperatorExpression
	| func=ID (functionArguments)	#FunctionExpression
	| op=unaryOperator expression	#UnaryOperatorExpression
	| id=ID		#IdentificatorExpression
	| value=constant	#ConstantExpression
	;	

functionArguments: expression (COMMA expression)* | ;


/*
 * Lexer Rules
 */


fragment LOWERCASE  : [a-z] ;
fragment UPPERCASE  : [A-Z] ;
fragment DIGIT: [0-9];
/*fragment A : [aA]; 
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];*/


/*RESISTOR			:RESISTOR;
VOLTAGESOURCE		:VOLTAGESOURCE;
VOLTAGE				:VOLTAGE;
CURRENT				:CURRENT;
LINE				:LINE;
CURRENTSOURCE		:CURRENTSOURCE;
CAPACITOR			:CAPACITOR;
GROUND				:GROUND;*/

INT: DIGIT+ ;
FLOAT: (DIGIT+ '.' DIGIT* |'.' DIGIT+)[Ee][+-]? DIGIT+; 
ID		: [_]*LOWERCASE[A-Za-z0-9_]*;
STRING	: '"' .*? '"'|'\'' .*? '\'';
NEWLINE	: ('\r'?'\n' | '\r')+ ;
WHITESPACE : (' '|'\t')+ -> skip ;
COMMENT 
	:	( '//' ~[\r\n]* ('\r'? '\n' | 'r')
		| '/*' .*? '*/'
		) -> skip
	;

PLUS               : '+' ;
MINUS              : '-' ;
ASTERISK           : '*' ;
DIVISION           : '/' ;
ASSIGN             : '=' ;
LPAREN             : '(' ;
RPAREN				: ')';
DOT					: '.';
COMMA				: ',' ;
SEMICOLON			: ';' ;
COLON				: ':' ;
LSQRPAREN			: '[' ;
RSQRPAREN			: ']' ;
LCRLPAREN			: '{' ;
RCRLPAREN			: '}' ;
ANGLE				:'@' ;
IM					:[Ii];