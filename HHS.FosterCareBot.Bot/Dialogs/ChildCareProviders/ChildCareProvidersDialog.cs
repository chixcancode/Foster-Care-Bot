using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HHS.FosterCareBot.Bot.Dialogs
{
    public class ChildCareProvidersDialog : Dialog
    {
        public const string Name = "ChildCareProviders";
        public ChildCareProvidersDialog()
           : base(Name)
        {
        }


        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = dc.Context.Activity.CreateReply();
            activity.Text = "Here is a list of 4 child providers in your area, you can click contact button to call provider directly.";
            activity.Speak = "I've found 4 child care providers in your area.";
            activity.Attachments = GetChildProvidersListCard();
            await dc.Context.SendActivityAsync(activity).ConfigureAwait(false);
             return await dc.EndDialogAsync();

        }


        private static List<Attachment> GetChildProvidersListCard()
        {
            List<Attachment> thumbnailCards = new List<Attachment>();
            var thumbnailCard = new ThumbnailCard
            {
                Title = "Kathy Calhoun - 4.8/5",
                Subtitle = "ABC Child Care Center",
                Text = "Kathy has over 25 years of child care experience and is a great nurturer.  Get 20% off per child visit",
                Images = new List<CardImage> { new CardImage("https://cf-lowcountry.org/Portals/0/Uploads/Images/Directory/Cindy%20Circle_20150106025709.jpg") },
                Buttons = new List<CardAction> { new CardAction() { Type = "call", Title = "Contact Kathy", Value = "skype:kathychildprovider@outlook.com" } },

            };

            thumbnailCards.Add(thumbnailCard.ToAttachment());
            var thumbnailCard1 = new ThumbnailCard
            {
                Title = "Jeremy Aupry - 4.2/5",
                Subtitle = "XYZ Child Care Center",
                Text = "Jeremy also loves to make homemade baby food.  Get 10% off per child visit.",
                Images = new List<CardImage> { new CardImage("https://airbooth.io/wp-content/uploads/2016/01/ouassim-profil-circle.png") },
                Buttons = new List<CardAction> { new CardAction() { Type = "call", Title = "Contact Jeremy", Value = "skype:chixcancode" } },

            };

            thumbnailCards.Add(thumbnailCard1.ToAttachment());
            var thumbnailCard2 = new ThumbnailCard
            {
                Title = "Thomas Beaulieu - 4/5",
                Subtitle = "Contoso Child Care Center",
                Text = "Thomas is a recent college grad from University of Maryland.  Get 10% off per child visit.",
                Images = new List<CardImage> { new CardImage("https://media.lpgenerator.ru/images/175411/komment5.png") },
                Buttons = new List<CardAction> { new CardAction() { Type = "call", Title = "Contact Thomas", Value = "skype:chixcancode" } },
            };

            thumbnailCards.Add(thumbnailCard2.ToAttachment());

            var thumbnailCard3 = new ThumbnailCard
            {
                Title = "Elizabeth Paulet - 3.8/5",
                Subtitle = "Paulet Child Care Center",
                Text = "Elizabeth has recently written a book on parenting newborns.  Get 10% off first visit.",
                Images = new List<CardImage> { new CardImage("https://www.awid.org/sites/default/files/thumbnails/image/idahot-charlese-circle.png") },
                Buttons = new List<CardAction> { new CardAction() { Type = "call", Title = "Contact Elizabeth", Value = "skype:chixcancode" } },
            };

            thumbnailCards.Add(thumbnailCard3.ToAttachment());
            return thumbnailCards;
        }
    }
}
