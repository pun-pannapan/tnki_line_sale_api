using tnki_line_sale_api.Constant;
using Line.Messaging;

namespace tnki_line_sale_api.Services
{
    public class LineAndMailService
    {
        public System.Globalization.CultureInfo _cultureTHInfo = new System.Globalization.CultureInfo("th-TH");
        public async Task sendLine_Wait(string lineUid, string store_name, string reqDateStr)
        {
            try
            {
                BubbleContainer containner = new BubbleContainer();
                containner.Body = new BoxComponent();
                containner.Body.Layout = BoxLayout.Vertical;
                containner.Body.Flex = 0;
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "ใบเสร็จอยู่ระหว่างการตรวจสอบ​​​",
                    Size = ComponentSize.Md,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Weight = Weight.Bold,
                    Wrap = true
                });

                containner.Body.Contents.Add(new SeparatorComponent()
                {
                    Margin = Spacing.Xl
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "กิจกรรม: โอโม Back to School​",
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "ร้านค้า: ​" + store_name,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });

                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "วันที่: ​" + reqDateStr,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
              

                containner.Body.Contents.Add(new ButtonComponent()
                {
                    Action = new UriTemplateAction("ดูประวัติของคุณ​​​​", ENUM_SERVERURL.frontURL_Profile),
                    Style = ButtonStyle.Primary,
                    Color = "#61bceb",
                    Height = ButtonHeight.Sm,
                    Margin = Spacing.Xl
                });

                var bubble = new FlexMessage("ใบเสร็จอยู่ระหว่างการตรวจสอบ​​")
                {
                    Contents = containner
                };

                LineMessagingClient lineMessagingClient = new LineMessagingClient(ENUM_LINE_MSG_API.lineOA);
                await lineMessagingClient.PushMessageAsync(lineUid, new FlexMessage[] { bubble });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        public async Task sendLine_Approve( string lineUid, int recPoint, string store_name, string recNo, string reqDateStr)
        {
            try
            {
                BubbleContainer containner = new BubbleContainer();

                containner.Body = new BoxComponent();
                containner.Body.Layout = BoxLayout.Vertical;
                containner.Body.Flex = 0;

                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "ใบเสร็จผ่านการอนุมัติแล้ว​",
                    Size = ComponentSize.Md,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Weight = Weight.Bold,
                    Wrap = true
                });

                containner.Body.Contents.Add(new SeparatorComponent()
                {
                    Margin = Spacing.Xl
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "กิจกรรม: โอโม Back to School​",
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "ร้านค้า: ​"+ store_name,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "เลขที่ใบเสร็จ: ​" + recNo,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "วันที่: ​" + reqDateStr,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "จำนวนคะแนนที่ได้รับ: ​" + recPoint,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });

                containner.Body.Contents.Add(new ButtonComponent()
                {
                    Action = new UriTemplateAction("ดูประวัติของคุณ​​", ENUM_SERVERURL.frontURL_Profile),
                    Style = ButtonStyle.Primary,
                    Color = "#61bceb",
                    Height = ButtonHeight.Sm,
                    Margin = Spacing.Xl
                });

                var bubble = new FlexMessage("ใบเสร็จผ่านการอนุมัติแล้ว")
                {
                    Contents = containner
                };

                LineMessagingClient lineMessagingClient = new LineMessagingClient(ENUM_LINE_MSG_API.lineOA);
                await lineMessagingClient.PushMessageAsync(lineUid, new FlexMessage[] { bubble });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
        public async Task sendLine_Reject(string lineUid, Guid req_guid, string store_name, string reqDateStr, string remark)
        {
            try
            {
                BubbleContainer containner = new BubbleContainer();
                containner.Body = new BoxComponent();
                containner.Body.Layout = BoxLayout.Vertical;
                containner.Body.Flex = 0;
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "ใบเสร็จไม่ผ่านการอนุมัติ​​",
                    Size = ComponentSize.Md,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Weight = Weight.Bold,
                    Wrap = true
                });

                containner.Body.Contents.Add(new SeparatorComponent()
                {
                    Margin = Spacing.Xl
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "กิจกรรม: โอโม Back to School​",
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "ร้านค้า: ​" + store_name,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
               
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "วันที่: ​" + reqDateStr,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "เหตุผล: ​" + remark,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });

                containner.Body.Contents.Add(new ButtonComponent()
                {
                    Action = new UriTemplateAction("แก้ไขใบเสร็จ​​​", ENUM_SERVERURL.frontURL_UploadRecEdit + req_guid),
                    Style = ButtonStyle.Primary,
                    Color = "#61bceb",
                    Height = ButtonHeight.Sm,
                    Margin = Spacing.Xl
                });

                var bubble = new FlexMessage("ใบเสร็จไม่ผ่านการอนุมัติ​")
                {
                    Contents = containner
                };

                LineMessagingClient lineMessagingClient = new LineMessagingClient(ENUM_LINE_MSG_API.lineOA);
                await lineMessagingClient.PushMessageAsync(lineUid, new FlexMessage[] { bubble });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        public async Task sendLine_RedeemVCSuccess(string lineUid, int usePoint, int redeemQty, string redeemName)
        {
            try
            {
                BubbleContainer containner = new BubbleContainer();

                containner.Body = new BoxComponent();
                containner.Body.Layout = BoxLayout.Vertical;
                containner.Body.Flex = 0;

                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "แลกของรางวัลสำเร็จ​",
                    Size = ComponentSize.Md,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Weight = Weight.Bold,
                    Wrap = true
                });

                containner.Body.Contents.Add(new SeparatorComponent()
                {
                    Margin = Spacing.Xl
                });

                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "กิจกรรม: โอโม Back to School​",
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "ของรางวัล: ​" + redeemName,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "จำนวน: ​" + redeemQty,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "จำนวนคะแนนที่ใช้: ​" + usePoint,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });

                containner.Body.Contents.Add(new ButtonComponent()
                {
                    Action = new UriTemplateAction("ดูประวัติของคุณ​​", ENUM_SERVERURL.baseFront_HistoryRedeem),
                    Style = ButtonStyle.Primary,
                    Color = "#61bceb",
                    Height = ButtonHeight.Sm,
                    Margin = Spacing.Xl
                });

                var bubble = new FlexMessage("แลกของรางวัลสำเร็จ")
                {
                    Contents = containner
                };

                LineMessagingClient lineMessagingClient = new LineMessagingClient(ENUM_LINE_MSG_API.lineOA);
                await lineMessagingClient.PushMessageAsync(lineUid, new FlexMessage[] { bubble });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        public async Task sendLine_UpdateTrackingSuccess(string lineUid, string trackingNo, string redeemName)
        {
            try
            {
                BubbleContainer containner = new BubbleContainer();

                containner.Body = new BoxComponent();
                containner.Body.Layout = BoxLayout.Vertical;
                containner.Body.Flex = 0;

                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "จัดส่งของรางวัลสำเร็จ​",
                    Size = ComponentSize.Md,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Weight = Weight.Bold,
                    Wrap = true
                });

                containner.Body.Contents.Add(new SeparatorComponent()
                {
                    Margin = Spacing.Xl
                });

                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "กิจกรรม: โอโม Back to School​",
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "ของรางวัล: ​" + redeemName,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
                containner.Body.Contents.Add(new TextComponent()
                {
                    Text = "เลขที่การจัดส่ง: ​" + trackingNo,
                    Size = ComponentSize.Sm,
                    Margin = Spacing.Xs,
                    Color = "#00379a",
                    Wrap = true
                });
               

                containner.Body.Contents.Add(new ButtonComponent()
                {
                    Action = new UriTemplateAction("ดูประวัติของคุณ​​", ENUM_SERVERURL.baseFront_HistoryRedeem),
                    Style = ButtonStyle.Primary,
                    Color = "#61bceb",
                    Height = ButtonHeight.Sm,
                    Margin = Spacing.Xl
                });
                containner.Body.Contents.Add(new ButtonComponent()
                {
                    Action = new UriTemplateAction("ติดตามพัสดุ​​", ENUM_SERVERURL.deliveryOwner),
                    Style = ButtonStyle.Primary,
                    Color = "#61bceb",
                    Height = ButtonHeight.Sm,
                    Margin = Spacing.Sm
                });

                var bubble = new FlexMessage("จัดส่งของรางวัลสำเร็จ")
                {
                    Contents = containner
                };

                LineMessagingClient lineMessagingClient = new LineMessagingClient(ENUM_LINE_MSG_API.lineOA);
                await lineMessagingClient.PushMessageAsync(lineUid, new FlexMessage[] { bubble });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }
    }
}
