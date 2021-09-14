#ifndef UART_PROTOCOL_H
#define	UART_PROTOCOL_H

unsigned char UartCalculateChecksum(int msgFunction,int msgPayloadLength, unsigned char* msgPayload);

void UartEncodeAndSendMessage(int msgFunction,int msgPayloadLength, unsigned char* msgPayload);

void UartDecodeMessage(unsigned char c);

void UartProcessDecodedMessage(unsigned char function,unsigned char payloadLength, unsigned char* payload);

#define StateReceptionWaiting 0
#define StateReceptionFunctionMSB 1
#define StateReceptionFunctionLSB 2
#define StateReceptionPayloadLengthMSB 3
#define StateReceptionPayloadLengthLSB 4
#define StateReceptionPayload 5
#define StateReceptionCheckSum 6

#define Function_Text 0x0080
#define Function_Led 0x0020
#define Function_Telem 0x0030
#define Function_Vitesse 0x0040
#define Function_Etape 0x0050
#define SET_ROBOT_STATE 0x0051
#define SET_ROBOT_MANUAL_CONTROL 0x0052
#define POSITION_DATA 0x0061


void SetRobotState (unsigned char c);
void SetRobotAutoControlState (unsigned char c);

#endif	/* UART_PROTOCOL_H */

