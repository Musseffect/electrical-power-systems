grammar ModelGrammar;

/*
 * Parser Rules
 */
 
boolean	    : value=(TRUE|FALSE);
float		: value = FLOAT;
integer     : value = INT;
string: value = STRING;
complexExp	: left=(FLOAT|INT) type=(IM| ANGLE) right=(FLOAT|INT);
complex		: IM im=(FLOAT|INT);
constant	: 
 complexExp| complex|float| integer| string| boolean
;
typeKeyword	: type=(OBJECT_TYPE|BOOLEAN_TYPE|INTEGER_TYPE|FLOAT_TYPE|STRING_TYPE);

model: (statement)* MODEL COLON modelObject=object SEMICOLON ELEMENTS COLON (elementStatement)* CONNECTIONS COLON (connectionStatement)* EOF;


elementStatement: element = ID ASSIGN definition=object SEMICOLON;

connectionStatement: CONNECT LPAREN elementID1=ID DOT nodeID1=ID COMMA elementID2=ID DOT nodeID2=ID RPAREN SEMICOLON;

statement: expression SEMICOLON;

array: '<' type = typeKeyword '>' LSQRPAREN values=arrayValues RSQRPAREN;

arrayValues: expression (COMMA expression)* | ;

expression: LPAREN expression RPAREN #BracketExpression
	| value = constant	#ConstantExpression
	| obj = object #ObjectExpression
	| arr = array #ArrayExpression
	| id=ID	#IdentifierExpression
	| func=ID LPAREN functionArguments RPAREN	#FunctionExpression
	| left=expression DOT id=ID #FieldExpression
	| <assoc=right>op=(PLUS | MINUS) expression	#UnaryOperatorExpression
	| <assoc=right>op=NOT expression	#UnaryOperatorExpression
	| left=expression op=(DIVISION|ASTERISK) right=expression	#BinaryOperatorExpression
	| left=expression op=(PLUS|MINUS) right=expression	#BinaryOperatorExpression
	| left=expression op=(OR|AND) right=expression	#BinaryOperatorExpression
	|<assoc=right> lvalue=expression ASSIGN rvalue=expression #AssignmentExpression
	;
	
functionArguments: expression (COMMA expression)* | ;
object: name=ID LCRLPAREN arguments RCRLPAREN;
arguments: keyValue (COMMA keyValue)* | ;
keyValue: key=ID ASSIGN value=expression;


/*
 * Lexer Rules
 */


fragment LOWERCASE  : [a-z] ;
fragment UPPERCASE  : [A-Z] ;
fragment DIGIT: [0-9] ;


CONNECTIONS: 'connections';
CONNECT: 'connect';
ELEMENTS: 'elements';
MODEL: 'model';
TRUE: 'true';
FALSE: 'false';
NOT: 'not';
OR: 'or';
AND: 'and';
BOOLEAN_TYPE: 'bool';
STRING_TYPE: 'string';
INTEGER_TYPE: 'int';
FLOAT_TYPE: 'float';
OBJECT_TYPE: 'object';


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
