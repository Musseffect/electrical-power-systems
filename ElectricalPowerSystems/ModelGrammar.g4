grammar ModelGrammar;

/*
 * Parser Rules
 */

 
 //New Grammar
 /*
number		: value=(FLOAT|INT);
complexExp	: left=number type=(IM| ANGLE) right=number;
complex		: IM im=number;
constant	: value=number  #NumberConstant
	| value=complex #ComplexConstant
	| value=complexExp #ComplexExprConstant
	| value=STRING #StringConstant
;

 model:statements ELEMENTS COLON elements CONNECTIONS COLON connections EOF
 ;

 statements: (statement)* 
 ;

 elements: (elementStatement)*
;

elementStatement: element = ID ASSIGN elementID = ID LCRLPAREN arguments RCRLPAREN SEMICOLON
 | SEMICOLON;

arguments: keyValue (COMMA keyValue)* | ;
keyValue: key=ID ASSIGN value=expression;

connections: (connectionStatement)*
;

connectionStatement: CONNECT LPAREN elementID=ID DOT nodeID=ID RPAREN SEMICOLON
 | SEMICOLON;

statement: expression SEMICOLON #StatementRule
 | SEMICOLON #emptyStatement;

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
	
functionArguments: expression (COMMA expression)* | ;*/


 //example
/*
elements:
generator=Generator
{
	Vpeak=100.0f,
	Phase = 0.0f,
	Z = 0.001+j0.0001f,
	Type=Wye
};
transformer=Transformator
{
	WindingTypePrimary=Wye,
	WindingTypeSecondary=Delta,
	Zp = 0.01+j0.0001,
	Zs = 0.01+j0.0001,
	G = 1000.0,
	X = 1000.0
}
load=Load
{
	Za=100+10j,
	Zb=100+10j,
	Zc=100+10j,
	Type=Wye
};
res = Resistance
{
	R=1000.0f
};
ground = Ground{};
meter1 = Wattmeter{Label = "generator"};
meter2 = Wattmeter{Label = "load"};

connections:
connect(generator.abc_out,meter1.abc_in);
connect(generator.n,res.in);
connect(res.out,ground.in);
connect(meter1.abc_out,transformer.abc_in);
connect(transformer.abc_out,meter2.abc_in);
connect(meter2.abc_out,load.abc_in);
connect(load.n,ground.in);
*/
 
 

 
number		: value=(FLOAT|INT);
complexExp	: left=number type=(IM| ANGLE) right=number;
complex		: IM im=number;
constant	: value=number  #NumberConstant
	| value=complex #ComplexConstant
	| value=complexExp #ComplexExprConstant
	| value=STRING #StringConstant
;

model: (state=statement)* EOF;



statement: expression SEMICOLON #StatementRule
| SEMICOLON #EmptyStatement;

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


/*
 * Lexer Rules
 */


fragment LOWERCASE  : [a-z] ;
fragment UPPERCASE  : [A-Z] ;
fragment DIGIT: [0-9] ;

FLOAT: (DIGIT+ DOT DIGIT*) ([Ee][+-]? DIGIT+)?
	   |DOT DIGIT+ ([Ee][+-]? DIGIT+)?
		|DIGIT+ ([Ee] [+-]? DIGIT+)?
		;
INT: DIGIT+ ; 
IM					: [Jj] ;
ID		: [_]*(LOWERCASE|UPPERCASE)[A-Za-z0-9_]*;

CONNECTIONS: 'connections';
CONNECT: 'connect';
ELEMENTS: 'elements';

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
