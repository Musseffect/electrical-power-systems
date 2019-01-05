grammar ModelGrammar;

/*
 * Parser Rules
 */

number		: value=(FLOAT|INT);
complexExp	: left=number type=(IM| ANGLE) right=number;
complex		: IM im=number;
constant	: value=number  #NumberConstant
	| value=complex #ComplexConstant
	| value=complexExp #ComplexExprConstant
	| value=STRING #StringConstant
;

model: (state=statement)*;


statement: expression SEMICOLON #StatementRule
| SEMICOLON #EmptyStatement;

unaryOperator: op=(PLUS | MINUS);

expression: op=unaryOperator expression	#UnaryOperatorExpression
	| left=expression op=(DIVISION|ASTERICS) right=expression	#BinaryOperatorExpression
	| left=expression op=(PLUS|MINUS) right=expression	#BinaryOperatorExpression
	|<assoc=right> left=expression op=CARET  right=expression	#BinaryOperatorExpression
	|<assoc=right> lvalue=expression ASSIGN rvalue=expression #AssignmentExpression
	| func=ID LPAREN functionArguments RPAREN	#FunctionExpression
	| id=ID		#IdentifierExpression
	| value=constant	#ConstantExpression
	| LPAREN expression RPAREN #BracketExpression
	| left=expression DOT id=ID #MemberExpression
	| (id=ID) right=expression #CastExpression
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
CARET				:'^';