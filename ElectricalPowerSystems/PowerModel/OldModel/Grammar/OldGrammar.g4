grammar OldGrammar;

number		: value=(FLOAT|INT);
complexExp	: left=number type=(IM| ANGLE) right=number;
complex		: IM im=number;
constant	: value=number  #NumberConstant
	| value=complex #ComplexConstant
	| value=complexExp #ComplexExprConstant
	| value=STRING #StringConstant
;

model: (state=statement)* EOF;



statement: expression SEMICOLON;

unaryOperator: op=(PLUS | MINUS);

expression: <assoc=right> left=expression op=CARET  right=expression	#BinaryOperatorExpression
	| LPAREN expression RPAREN #BracketExpression
	| func=ID LPAREN functionArguments RPAREN	#FunctionExpression
	| left=expression DOT id=ID #FieldExpression
	| op=unaryOperator expression	#UnaryOperatorExpression
	| LPAREN id=ID RPAREN right=expression #CastExpression
	| left=expression op=(DIVISION|ASTERISK) right=expression	#BinaryOperatorExpression
	| left=expression op=(PLUS|MINUS) right=expression	#BinaryOperatorExpression
	|<assoc=right> lvalue=expression ASSIGN rvalue=expression #AssignmentExpression
	| id=ID		#IdentifierExpression
	| value=constant	#ConstantExpression
	;	


functionArguments: expression (COMMA expression)* | ;

//end


/*
 * Lexer Rules
 */


fragment LOWERCASE  : [a-z] ;
fragment UPPERCASE  : [A-Z] ;
fragment DIGIT: [0-9] ;


INT: DIGIT+ ; 
FLOAT: (DIGIT+ DOT DIGIT*) ([Ee][+-]? DIGIT+)?
	   |DOT DIGIT+ ([Ee][+-]? DIGIT+)?
		|DIGIT+ ([Ee] [+-]? DIGIT+)?
		;
IM					: [Jj] ;
ID		: [_]*(LOWERCASE|UPPERCASE)[A-Za-z0-9_]*;

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
CARET				:'^';

STRING	: '"' .*? '"'|'\'' .*? '\'';
NEWLINE	: ('\r'? '\n' | '\r')+ -> skip;
WHITESPACE : (' ' | '\t')+ -> skip ;
COMMENT 
	:	( '//' ~[\r\n]* ('\r'? '\n' | 'r')
		| '/*' .*? '*/'
		) -> skip
	;
