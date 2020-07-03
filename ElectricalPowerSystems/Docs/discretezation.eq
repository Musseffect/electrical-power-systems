constant x0 = 1;
constant x14 = 1.8;
constant t0 = 0;
constant t14 = 1;
constant N = 15;
constant dt = (t14 - t0)/(N-1);
/*
der(x)=sin(x+t)
x(t0) = x0;
x(t1) = x1;
der(x(t)) ~= (x(t+dt)-x(t-dt))/(2dt)
*/

- sin(x1 + t0 + dt)*2*dt + x2 - x0  = 0;
x3 - x1 - sin(x2 + t0 + dt*2)*2*dt = 0;
x4 - x2 - sin(x3 + t0 + dt*3)*2*dt = 0;
x5 - x3 - sin(x4 + t0 + dt*4)*2*dt = 0;
x6 - x4 - sin(x5 + t0 + dt*5)*2*dt = 0;
x7 - x5 - sin(x6 + t0 + dt*6)*2*dt = 0;
x8 - x6 - sin(x7 + t0 + dt*7)*2*dt = 0;
x9 - x7 - sin(x8 + t0 + dt*8)*2*dt = 0;
x10 - x8 - sin(x9 + t0 + dt*9)*2*dt = 0;
x11 - x9 - sin(x10 + t0 + dt*10)*2*dt = 0;
x12 - x10 - sin(x11 + t0 + dt*11)*2*dt = 0;
x13 - x11 - sin(x12 + t0 + dt*12)*2*dt = 0;
x14 - x12 - sin(x13 + t0 + dt*13)*2*dt = 0;

x1(0) = 1;
x2(0) = 1.1;
x3(0) = 1.2;
x4(0) = 1.2;
x5(0) = 1.2;
x6(0) = 1.3;
x7(0) = 1.3;
x8(0) = 1.4;
x9(0) = 1.4;
x10(0) = 1.5;
x11(0) = 1.6;
x12(0) = 1.6;
x13(0) = 1.7;