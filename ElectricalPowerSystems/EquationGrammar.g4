grammar EquationGrammar;

/*
 * Parser Rules
 */
number		: value=(FLOAT|INT);

compileUnit
	:(state=statement)*	EOF
	;

statement: eq=equation SEMICOLON #StatementRule
| SEMICOLON #EmptyStatement;

equation: left=expression ASSIGN right=expression #EquationRule
	| 'set' id=ID ASSIGN right=expression #ParameterRule
	| id=ID '[0]' ASSIGN expression #InitialValueAssignmentRule;

unaryOperator: op=(PLUS | MINUS);

expression: <assoc=right> left=expression op=CARET  right=expression	#BinaryOperatorExpression
	| LPAREN expression RPAREN #BracketExpression
	| func=ID LPAREN functionArguments RPAREN	#FunctionExpression
	| op=unaryOperator expression	#UnaryOperatorExpression
	| left=expression op=(DIVISION|ASTERISK) right=expression	#BinaryOperatorExpression
	| left=expression op=(PLUS|MINUS) right=expression	#BinaryOperatorExpression
	| id=ID #IdentifierExpression
	| value=number	#ConstantExpression
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
		;
INT: DIGIT+ ; 
ID: [_]*(LOWERCASE|UPPERCASE)[A-Za-z0-9_]*;


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