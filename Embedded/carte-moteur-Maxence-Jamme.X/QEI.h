#ifndef QEI_H
#define	QEI_H

#define DISTROUES 0.217
#define FREQ_ECH_QEI 250
#define point_meter 0.0000163

void InitQEI1 ();
void InitQEI2 ();
void QEIUpdateData ();
void SendPositionData();

#endif

