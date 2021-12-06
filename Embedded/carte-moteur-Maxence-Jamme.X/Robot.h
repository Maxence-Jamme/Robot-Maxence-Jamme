#ifndef ROBOT_H
#define ROBOT_H

#include "asservissement.h"

typedef struct robotStateBITS {

    union {
        struct {
            unsigned char taskEnCours;
            float vitesseGaucheConsigne;
            float vitesseGaucheCommandeCourante;
            float vitesseDroiteConsigne;
            float vitesseDroiteCommandeCourante;
            float distanceTelemetreDroit;
            float distanceTelemetreCentre;
            float distanceTelemetreGauche;
            float distanceTelemetreExtremeDroit;
            float distanceTelemetreExtremeGauche;

            /**************** Odométrie ****************/
            double vitesseDroitFromOdometry;
            double vitesseGaucheFromOdometry;
            double vitesseLineaireFromOdometry;
            double vitesseAngulaireFromOdometry;
            double xPosFromOdometry_1;
            double yPosFromOdometry_1;
            double xPosFromOdometry;
            double yPosFromOdometry;
            double angleRadianFromOdometry_1;
            double vitesseLineaireFromOdometry_1;
            double vitesseAngulaireFromOdometry_1;
            double angleRadianFromOdometry;     
            
            /**************** Asservissement ****************/
            PidCorrector PidX;
            PidCorrector PidTheta;
            
            /**************** Commande Moteur ****************/
            double vitesseAngulairePourcent;
            double thetaCorrectionVitesseCommande;
            double vitesseLineairePourcent;
            double xCorrectionVitesseCommande;
            double vitesseLineaireConsigne;
            double vitesseAngulaireConsigne;
            double xCorrectionVitessePourcent;
            double thetaCorrectionVitessePourcent;
           
        }
        ;
    }
    ;
} ROBOT_STATE_BITS;

extern volatile ROBOT_STATE_BITS robotState;
#endif /* ROBOT_H */