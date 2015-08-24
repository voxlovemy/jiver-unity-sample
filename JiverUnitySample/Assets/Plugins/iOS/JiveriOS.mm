
#import <JiverSDK/JiverSDK.h>
#import "JiveriOS.h"


// Converts C style string to NSString
NSString* CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

// Helper method to create C string copy
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

@implementation JiveriOS

- (id)init
{
    self = [super init];
    return self;
}

- (void)sendMessage:(NSString *)target andArg:(NSString *)arg
{
    UnitySendMessage(MakeStringCopy([self.unityResponder UTF8String]), MakeStringCopy([target UTF8String]), MakeStringCopy([arg UTF8String]));
}
@end


extern "C" {
    static JiveriOS* jiveriOS = nil;
    void _Jiver_iOS_Init(const char* appId, const char* responder)
    {
        if(jiveriOS == nil)
        {
            jiveriOS = [[JiveriOS alloc] init];
        }
        
        jiveriOS.unityResponder = CreateNSString(responder);
        [Jiver initAppId:CreateNSString(appId) selectDeviceId:kJiverInitWithIDFV];
        
        [Jiver setEventHandlerConnectBlock:^(JiverChannel *channel) {
            // Connected to JIVER channel
            [jiveriOS sendMessage: @"_OnConnect" andArg: [channel toJson]];
        } errorBlock:^(NSInteger code) {
            // Error occured due to bad APP_ID (or other unknown reason)
            [jiveriOS sendMessage: @"_OnError" andArg: [@(code) stringValue]];
            
        } channelLeftBlock:^(JiverChannel *channel) {
            
        } messageReceivedBlock:^(JiverMessage *message) {
            // Received a regular chat message
            [jiveriOS sendMessage: @"_OnMessageReceived" andArg: [message toJson]];
            
        } systemMessageReceivedBlock:^(JiverSystemMessage *message) {
            // Received a system message
            [jiveriOS sendMessage: @"_OnSystemMessageReceived" andArg: [message toJson]];
            
        } broadcastMessageReceivedBlock:^(JiverBroadcastMessage *message) {
            // Received a broadcast message
            [jiveriOS sendMessage: @"_OnBroadcastMessageReceived" andArg: [message toJson]];
            
        } fileReceivedBlock:^(JiverFileLink *fileLink) {
            // Received a file
            [jiveriOS sendMessage: @"_OnFileReceived" andArg: [fileLink toJson]];
            
        } messagingStartedBlock:^(JiverMessagingChannel *channel) {
            
        } messagingUpdatedBlock:^(JiverMessagingChannel *channel) {
            
        } messagingEndedBlock:^(JiverMessagingChannel *channel) {
            
        } messagingHiddenBlock:^(JiverMessagingChannel *channel) {
            
        } readReceivedBlock:^(JiverReadStatus *status) {
            
        } typeStartReceivedBlock:^(JiverTypeStatus *status) {
            
        } typeEndReceivedBlock:^(JiverTypeStatus *status) {
            
        } allDataReceivedBlock:^(NSUInteger jiverDataType, int count) {
            
        } messageDeliveryBlock:^(BOOL send, NSString *message, NSString *data, NSString *messageId) {
            
        }];
    }
    
    void _Jiver_iOS_Login(const char* uuid, const char* nickname)
    {
        [Jiver loginWithUserId:CreateNSString(uuid) andUserName:CreateNSString(nickname)];
    }
    
    void _Jiver_iOS_Join (const char* channelUrl)
    {
        [Jiver joinChannel:CreateNSString(channelUrl)];
    }
    
    void _Jiver_iOS_Connect (int prevMessageLimit)
    {
        //        [Jiver connect];
        [[Jiver queryMessageListInChannel:[Jiver getChannelUrl]] prevWithMessageTs:LLONG_MAX andLimit:prevMessageLimit resultBlock:^(NSMutableArray *queryResult) {
            long long mMaxMessageTs = LLONG_MIN;
            for (JiverMessageModel *model in queryResult) {
                if (mMaxMessageTs < [model getMessageTimestamp]) {
                    mMaxMessageTs = [model getMessageTimestamp];
                }
                if ([model isKindOfClass: [JiverMessage class]]) {
                    [jiveriOS sendMessage: @"_OnMessageReceived" andArg: [(JiverMessage*)model toJson]];
                }
                else if ([model isKindOfClass: [JiverSystemMessage class]]) {
                    [jiveriOS sendMessage: @"_OnSystemMessageReceived" andArg: [(JiverSystemMessage*)
                                                                                model toJson]];
                }
                else if ([model isKindOfClass: [JiverBroadcastMessage class]]) {
                    [jiveriOS sendMessage: @"_OnBroadcastMessageReceived" andArg: [(JiverBroadcastMessage*)model toJson]];
                }
                else if ([model isKindOfClass: [JiverFileLink class]]) {
                    [jiveriOS sendMessage: @"_OnFileReceived" andArg: [(JiverFileLink*)model toJson]];
                }
                
            }
            
            [Jiver connectWithMessageTs:mMaxMessageTs];
        } endBlock:^(NSError *error) {
            
        }];
    }
    
    void _Jiver_iOS_Disconnect ()
    {
        [Jiver disconnect];
    }
    
    void _Jiver_iOS_QueryChannelListForUnity ()
    {
        JiverChannelListQuery* query = [Jiver queryChannelListForUnity];
        [query nextWithResultBlock:^(NSMutableArray *queryResult) {
            NSMutableArray* jsonArray = [NSMutableArray array];
            for (JiverChannel* channel in queryResult) {
                [jsonArray addObject:[channel toJson]];
            }
            
            NSError* error = [[NSError alloc] init];
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:jsonArray options:NSJSONWritingPrettyPrinted error:&error];
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            
            [jiveriOS sendMessage: @"_OnQueryChannelList" andArg: jsonString];
        } endBlock:^(NSError *error) {
            
        }];
    }
    
    void _Jiver_iOS_Send (const char* message)
    {
        [Jiver sendMessage:CreateNSString(message)];
    }
    
    void _Jiver_iOS_SendWithData (const char* message, const char* data)
    {
        [Jiver sendMessage:CreateNSString(message) withData:CreateNSString(data)];
    }
}