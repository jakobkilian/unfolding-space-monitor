
// import UDP library
import hypermedia.net.*;

//SYSTEM_________
long[] lastCall = new long[4];  //saves the millis() of the last call
String[] inVal = new String[25];  //saves the millis() of the last call

float proFps;
int countFrames;

//USER_________
int callHz=20;        //how often should the Raspi be called?

long priTime;


long lastIncMsg=0;
boolean isPhone=false;



void pri(String temp) {
  print(temp);
  print ("   ");
  println(millis()-priTime);
  priTime=millis();
}

long mylong(String a)
{
  return Long.parseLong(a);
}

int myInt(String a)
{
  return Integer.parseInt(a);
}

//checks if there should be a new UDP call to the Raspberry
boolean myTimer(float thisHz, int thisId) {
  boolean tmp=false;
  if ((millis()-lastCall[thisId])>(1000.0f/thisHz)) {
    tmp = true;
    lastCall[thisId]=millis();
  }
  return tmp;
}


void myText(String thisText, int thisRow, int thisColumn) {
  PVector txt0 = new PVector(0, 0);
  PVector txtGap = new PVector(0, 0);
  fill(255);
  if (isPhone) {
    textSize(45);
    txt0 = new PVector(50, 30);
    txtGap = new PVector(50, 350);
  }
  if (!isPhone) {
    textSize(18);
    txt0 = new PVector(20, 20);
    txtGap = new PVector(24, 150);
  }
  if (thisText!=null)
    text(thisText, txt0.y + (thisColumn * txtGap.y), txt0.x + (thisRow * txtGap.x));
}


String current="";
UDP udp;  // define the UDP object
boolean printUdp=false;

String ip       = "192.168.1.12";  // the remote IP address
int port        = 9009;    // the destination port

String left  = "left";  // the message to send
String right  = "right" ;  // the message to send
//String ip       = "192.168.1.255";  // the remote IP address
int inPort        = 53333;    // the destination port
int outPort        = 52222;    // the destination port
String message="0";

/**
 * init
 */

// Run on Screen or Phone?
public void settings() {
  if (displayWidth>1200) {
    size(1000, 900);
    isPhone=false;
  } else {
    size(displayWidth, displayHeight);
    isPhone=true;
  }
}

void setup() {
  //TODO id device..
  frameRate(50);
  noStroke();
  fill(0);
  // create a new datagram connection on port 6000
  // and wait for incomming message
  udp = new UDP( this, 9012 );
  //udp.log( true );     // <-- printout the connection activity
  udp.listen( true );

  for (int i=0; i<inVal.length; i++) {
    inVal[i]="111";
  }
}

//process events
void draw() {
  //With fps of 30: send request do Raspi TODO: Ip static?
  if (myTimer(30, 1)) {
    udp.send( message, ip, port );
  }
  if (myTimer(0.25, 0))
  {
    proFps=countFrames/4;
    countFrames=0;
  }
  //print all msgs to the console
  if (printUdp){
  for (int i=0; i<25; i++) {
    print(inVal[i] + "\t");
  }
  println();}
  
  background(0);

  //Get get current time
  long sysTime= (System.currentTimeMillis());
  //calc the offset to last incoming message
  long offTime=(sysTime-lastIncMsg)/1000;


  if (offTime>0.9) {
    fill(100, 20, 20);
    rect(0, 0, 600, 55);      
    myText(str(int(offTime)) + "s", 0, 1);
  } else {
    fill(20, 100, 20);
    rect(0, 0, 600, 55);
    myText("now", 0, 1);
  }
  myText("last received", 0, 0);


  myText("last frame:", 1, 0);
  myText(inVal[1]+" ms", 1, 1);
  myText("longest fr.:", 2, 0);
  myText(inVal[2]+" ms", 2, 1);
  myText("frames:", 3, 0);
  myText(inVal[3]+" fps", 3, 2);
  myText(inVal[0], 3, 1);
  myText("distance:", 4, 0);
  if (inVal[4].length()>3) {
    myText(inVal[4].substring(0, 3)+" m", 4, 1);
  }

  if (int(inVal[6])==1) {
    myText("angeschlossen", 6, 1);
  } else {
    fill(100, 20, 20);
    rect(0, 313, 600, 45);
    myText("--", 6, 1);
  }
  myText("Kamera", 6, 0);
  if (int(inVal[7])==1) {
    myText("läuft", 7, 1);
  } else {
    fill(100, 20, 20);
    rect(0, 363, 600, 45);
    myText("--", 7, 1);
  }
  myText("Aufnahme:", 7, 0);
  myText("lib gecrashed:", 8, 0);
  myText(inVal[8]+" mal", 8, 1);
  myText("drops bridge:", 9, 0);
  myText(inVal[9], 9, 1);
  myText("drops FC:", 10, 0);
  myText(inVal[10], 10, 1);
  myText("drops in 10s:", 11, 0);
  myText(inVal[11], 11, 1);    
  myText("delivered:", 12, 0);
  myText(inVal[12] + "  frames", 12, 1);        
  myText("Cycle Zeit:", 13, 0);
  myText(inVal[13] + " ms", 13, 1);   
  myText("Pause Zeit:", 14, 0);
  myText(inVal[14] + " ms", 14, 1);   

  myText("Process FPS:", 16, 0);
  myText(str(proFps), 16, 1);   


  //tiles
  fill(255);
  int  tilSiz=int((width*0.6)/3);
  if (height/2<tilSiz*3) {
    tilSiz=int((height*0.4)/3);
  }
  int pad=tilSiz/4;
  int x0=int(width*0.15);
  int y0=height-((2*pad)+(3*tilSiz));
  int c=15;
  textAlign(CENTER);
  for (int y=0; y<3; y++) {
    for (int x=0; x<3; x++) {
      textSize(height/60);
      int bright=int(inVal[c]);
      fill(map(bright, 0, 255, 255, 0));
      rect(x0+pad+(x*tilSiz), y0+pad+(y*tilSiz), tilSiz, tilSiz);
      fill(255);
      if (bright<70) { 
        fill(0);
      }
      if (bright==0) {
        text("-", x0+pad+tilSiz*0.5+(x*tilSiz), y0+pad+tilSiz*0.5+(y*tilSiz));
      } else {
        text(bright, x0+pad+tilSiz*0.5+(x*tilSiz), y0+pad+tilSiz*0.5+(y*tilSiz));
      }
      c++;
    }
  }
  textAlign(LEFT);
}

void receive( byte[] data, String ip, int inPort ) {  // <-- extended handler
  lastIncMsg=System.currentTimeMillis();
  String temp = new String( data );

  countFrames++;
  //if string is not empty
  while (temp.length()>0) {
    //if string contains a |
    if (temp.indexOf("|")!=-1) {
      //get id of pipe
      int pipeId = temp.indexOf("|");
      //isolate elemt
      String element = temp.substring(0, pipeId);
      //if there is sth left after isolation: save it to temp
      if (temp.length()>pipeId+1) {
        temp = temp.substring(pipeId+1);
      } else {
        temp="";
      }
      //find end of id
      int identId = element.indexOf(":");
      int ident = myInt(element.substring(0, identId));
      inVal[ident]=element.substring(identId+1);
    }
  }
}
