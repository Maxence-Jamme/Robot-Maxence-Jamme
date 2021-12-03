#include "main.h"
#include "Toolbox.h"
#include "QEI.h"
#include <P33EP512GM306.h>
#include "Robot.h"
#include <math.h>
#include "timer.h"
#include "UART_Protocol.h"



//float point_meter = (0.0426 * PI)/8192;


double QeiDroitPosition_T_1 = 0;
double QeiDroitPosition = 0;
double QeiGauchePosition_T_1 = 0;
double QeiGauchePosition = 0;
double delta_d = 0;
double delta_g = 0;
double delta_theta = 0;
double dx = 0;

void InitQEI1 (){
    QEI1IOCbits.SWPAB = 1 ; //QEAx and QEBx are swapped
    QEI1GECL = 0xFFFF;
    QEI1GECH = 0xFFFF;
    QEI1CONbits.QEIEN = 1 ; // Enable QEI Module                                                                               
}

void InitQEI2 (){
    QEI2IOCbits.SWPAB = 1 ; //QEAx and QEBx are not swapped
    QEI2GECL = 0xFFFF;
    QEI2GECH = 0xFFFF;
    QEI2CONbits.QEIEN = 1 ; // Enable QEI Module
}

void QEIUpdateData (){
    //On sauvegarde les anciennes valeurs
    QeiDroitPosition_T_1 = QeiDroitPosition ;
    QeiGauchePosition_T_1 = QeiGauchePosition ;
    
    //On réactualise les valeurs des positions
    long QEI1RawValue = POS1CNTL;
    QEI1RawValue += ((long)POS1HLD<<16);
    long QEI2RawValue = POS2CNTL;
    QEI2RawValue += ((long)POS2HLD<<16);
    
    // Conversion en mm (règlé pour la taille des roues codeuses)
    QeiDroitPosition = point_meter * QEI1RawValue;
    QeiGauchePosition = -point_meter * QEI2RawValue;
    
    // Calcul des deltas de position
    delta_d = QeiDroitPosition - QeiDroitPosition_T_1 ;
    delta_g = QeiGauchePosition - QeiGauchePosition_T_1 ;
    
    // delta_theta = atan((delta_d - delta_g) / DISTROUES) ;
    delta_theta = (delta_d - delta_g ) / DISTROUES;
    dx = (delta_d + delta_g ) / 2 ;
    
    robotState.vitesseLineaireFromOdometry_1 = robotState.vitesseLineaireFromOdometry;
    robotState.vitesseAngulaireFromOdometry_1 = robotState.vitesseAngulaireFromOdometry;
    
    robotState.vitesseDroitFromOdometry = delta_d*FREQ_ECH_QEI;
    robotState.vitesseGaucheFromOdometry = delta_g*FREQ_ECH_QEI;
    
    robotState.vitesseLineaireFromOdometry = (robotState.vitesseDroitFromOdometry + robotState.vitesseGaucheFromOdometry)/2;
    robotState.vitesseAngulaireFromOdometry = delta_theta*FREQ_ECH_QEI;

    //Mise Ã  jour du postionnement  terrain Ã  t-1
    robotState.xPosFromOdometry_1 = robotState.xPosFromOdometry;
    robotState.yPosFromOdometry_1 = robotState.yPosFromOdometry;
    robotState.angleRadianFromOdometry_1 = robotState.angleRadianFromOdometry;

    // Calcul des positions dans le referentiel du terrain
    robotState.xPosFromOdometry = robotState.xPosFromOdometry + (robotState.vitesseLineaireFromOdometry/FREQ_ECH_QEI )* cos(robotState.angleRadianFromOdometry);
    robotState.yPosFromOdometry = robotState.yPosFromOdometry + (robotState.vitesseLineaireFromOdometry/FREQ_ECH_QEI )* sin(robotState.angleRadianFromOdometry);
    robotState.angleRadianFromOdometry = robotState.angleRadianFromOdometry + delta_theta; 
    if ( robotState.angleRadianFromOdometry > PI )
        robotState.angleRadianFromOdometry -= 2*PI ;
    if ( robotState.angleRadianFromOdometry < -PI )
        robotState.angleRadianFromOdometry += 2*PI ;
    /*-------------------------
     * 
     * 
    // Calcul des vitesses
    // attention à remultiplier par la fréquence d'échantillonnage
    robotState.vitesseDroitFromOdometry = delta_d * FREQ_ECH_QEI;
    robotState.vitesseGaucheFromOdometry = delta_g * FREQ_ECH_QEI;
    robotState.vitesseLineaireFromOdometry = (robotState.vitesseDroitFromOdometry + robotState.vitesseGaucheFromOdometry) / 2 ;
    robotState.vitesseAngulaireFromOdometry = delta_theta * FREQ_ECH_QEI;
    
    //Mise à jour du positionnement terrain à t-1
    robotState.xPosFromOdometry_1 = robotState.xPosFromOdometry ;
    robotState.yPosFromOdometry_1 = robotState.yPosFromOdometry ;
    robotState.angleRadianFromOdometry_1 = robotState.angleRadianFromOdometry ;
    
    // Calcul des positions dans le referentiel du terrain
    robotState.xPosFromOdometry = robotState.xPosFromOdometry_1 + (robotState.vitesseLineaireFromOdometry/FREQ_ECH_QEI) * cos(robotState.angleRadianFromOdometry);
    robotState.yPosFromOdometry = robotState.yPosFromOdometry_1 + (robotState.vitesseLineaireFromOdometry/FREQ_ECH_QEI) * sin(robotState.angleRadianFromOdometry);
    robotState.angleRadianFromOdometry = robotState.angleRadianFromOdometry + delta_theta;
    if (robotState.angleRadianFromOdometry > PI ){
        robotState.angleRadianFromOdometry -= 2 * PI ;
    }
    if (robotState.angleRadianFromOdometry < -PI ){
        robotState.angleRadianFromOdometry += 2 * PI ;
    }
     
     
     */
}


void SendPositionData()
{
    unsigned char positionPayload [24] ;
    getBytesFromInt32 (positionPayload, 0, timestamp);
    getBytesFromFloat (positionPayload, 4, (float) (robotState.xPosFromOdometry));
    getBytesFromFloat (positionPayload, 8, (float) (robotState.yPosFromOdometry));
    getBytesFromFloat (positionPayload, 12, (float) (robotState.angleRadianFromOdometry));
    getBytesFromFloat (positionPayload, 16, (float) (robotState.vitesseLineaireFromOdometry));
    getBytesFromFloat (positionPayload, 20, (float) (robotState.vitesseAngulaireFromOdometry));
    UartEncodeAndSendMessage (POSITION_DATA, 24, positionPayload);
}
