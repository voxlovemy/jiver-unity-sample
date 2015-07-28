
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
        [Jiver initWithAppKey:CreateNSString(appId)];
        
        [Jiver setEventHandlerConnectBlock:^(Channel *channel) {
            // Connected to JIVER channel
            [jiveriOS sendMessage: @"_OnConnect" andArg: [channel toJson]];
        } errorBlock:^(int code) {
            // Error occured due to bad APP_ID (or other unknown reason)
            [jiveriOS sendMessage: @"_OnError" andArg: [@(code) stringValue]];
        } messageReceivedBlock:^(Message *message) {
            // Received a regular chat message
            [jiveriOS sendMessage: @"_OnMessageReceived" andArg: [message toJson]];
        } systemMessageReceivedBlock:^(SystemMessage *message) {
            // Received a system message
            [jiveriOS sendMessage: @"_OnSystemMessageReceived" andArg: [message toJson]];
        } broadcastMessageReceivedBlock:^(BroadcastMessage *message) {
            // Received a broadcast message
            [jiveriOS sendMessage: @"_OnBroadcastMessageReceived" andArg: [message toJson]];
        } fileReceivedBlock:^(FileLink *fileLink) {
            // Received a file
            [jiveriOS sendMessage: @"_OnFileReceived" andArg: [fileLink toJson]];
        } messagingStartedBlock:^(MessagingChannel *channel) {
            // Callback for [Jiver startMessagingWithUserId:]
            [jiveriOS sendMessage: @"_OnMessagingStarted" andArg: [channel toJson]];
        } messagingEndedBlock:^(MessagingChannel *channel) {
            // Callback for [Jiver endMessagingWithChannelUrl:]
            [jiveriOS sendMessage: @"_OnMessagingEnded" andArg: [channel toJson]];
        } readReceivedBlock:^(ReadStatus *status) {
            // Callback for read status changes
            [jiveriOS sendMessage: @"_OnReadReceived" andArg: [status toJson]];
        } typeStartReceivedBlock:^(TypeStatus *status) {
            // Callback for type status changes
            [jiveriOS sendMessage: @"_OnTypeStartReceived" andArg: [status toJson]];
        } typeEndReceivedBlock:^(TypeStatus *status) {
            // Callback for type status changes
            [jiveriOS sendMessage: @"_OnTypeEndReceived" andArg: [status toJson]];
        } messagesLoadedBlock:^(int count) {
            // Callback for [Jiver loadMoreMessagesWithLimit:]
            [jiveriOS sendMessage: @"_OnMessagesLoaded" andArg: [@(count) stringValue]];
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
    
    void _Jiver_iOS_Connect ()
    {
        [Jiver connect];
    }
    
    void _Jiver_iOS_Disconnect ()
    {
        [Jiver disconnect];
    }
    
    void _Jiver_iOS_QueryChannelListForUnity ()
    {
        ChannelListQuery* query = [Jiver queryChannelListForUnity];
        [query nextWithResultBlock:^(NSMutableArray *queryResult) {
            NSMutableArray* jsonArray = [NSMutableArray array];
            for (Channel* channel in queryResult) {
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