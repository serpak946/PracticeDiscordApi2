using Refit;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Maui.Controls;
using static Microsoft.Maui.ApplicationModel.Permissions;
using System.Collections.Generic;
using System.Threading.Channels;

namespace PracticeDiscordApi2
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        [Serializable]
        public class Guild
        {
            public string id { get; set; }
            public string name { get; set; }
            public string icon { get; set; }
            public string icon_hash { get; set; }
            public string splash { get; set; }
            public string discovery_splash { get; set; }
            public bool owner { get; set; }
            public string owner_id { get; set; }
            //public string permissions { get; set;}
            public string afk_channel_id { get; set; }
            public int afk_timeout { get; set; }
            public bool widget_enabled { get; set; }
            public string widget_channel_id { get; set; }
            public int verification_level { get; set; }
            public int default_message_notifications { get; set; }
            public int explicit_content_filter { get; set; }
            public override string ToString()
            {
                return name.ToString();
            }
        }
        public enum Channel_Types
        {
            GUILD_TEXT = 0,
            DM = 1,
            GUILD_VOICE = 2,
            GROUP_DM = 3,
            GUILD_CATEGORY = 4,
            GUILD_ANNOUNCEMENT = 5,
            ANNOUNCEMENT_THREAD = 10,
            PUBLIC_THREAD = 11,
            PRIVATE_THREAD = 12,
            GUILD_STAGE_VOICE = 13,
            GUILD_DIRECTORY = 14,
            GUILD_FORUM = 15,
            GUILD_MEDIA = 16
        }

        [Serializable]
        public class Channel
        {
            public string id { get; set; }
            public Channel_Types type { get; set; }
            public string guild_id { get; set; }
            public int position { get; set; }
            public string name { get; set; }
            public string parent_id { get; set; }
            public override string ToString()
            {
                return name.ToString();
            }
        }

        [Serializable]
        public class User
        {
            public string id { get; set; }
            public string username { get; set; }
            public string discriminator { get; set; }
            public string global_name { get; set; }
            public string avatar { get; set; }
            public bool bot { get; set; }
            public bool system { get; set; }
            public bool mfa_enabled { get; set; }
            public string banner { get; set; }
            public string locale { get; set; }
            public bool verified { get; set; }
            public string email { get; set; }
            public int flags { get; set; }
            public int premium_type { get; set; }
            public int public_flags { get; set; }
            public object avatar_decoration_data { get; set; }
        }

        public class Token
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string scope { get; set; }
            public string id_token { get; set; }
        }

        public interface IDiscordApi
        {
            [Headers("Content-Type: application/x-www-form-urlencoded")]
            [Post("/oauth2/token")]
            Task<Token> GetToken([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> tokenRequest);
            [Get("/users/@me")]
            Task<User> GetCurrentUser([Header("Authorization")] string token);
            [Get("/users/@me/guilds")]
            Task<List<Guild>> GetCurrentUserGuilds([Header("Authorization")] string token);

            [Headers("Content-Type: application/json, User-Agent: testPractice")]
            [Put("/guilds/{guildId}/members/{memberId}")]
            Task<HttpResponseMessage> InviteCurrentUser([Header("Authorization")] string token, [Body(BodySerializationMethod.Serialized)] Dictionary<string, string> accessToken, string guildId, string memberId);


            [Post("/guilds")]
            Task<Guild> CreateGuild([Header("Authorization")] string token, [Body(BodySerializationMethod.Serialized)] Dictionary<string, string> guild);
            
            [Headers("User-Agent: testPractice")]
            [Patch("/guilds/{guildId}")]
            Task<Guild> ModifyGuild([Header("Authorization")] string token, string guildId, [Body(BodySerializationMethod.Serialized)] Dictionary<string, string> guild);

            [Headers("User-Agent: testPractice")]
            [Delete("/guilds/{guildId}")]
            Task<HttpResponseMessage> DeleteGuild([Header("Authorization")] string token, string guildId);

            [Headers("User-Agent: testPractice")]
            [Get("/guilds/{guildId}/channels")]
            Task<List<Channel>> GetChannels([Header("Authorization")] string token, string guildId);

            [Headers("User-Agent: testPractice")]
            [Post("/guilds/{guildId}/channels")]
            Task<Channel> CreateChannel([Header("Authorization")] string token, [Body(BodySerializationMethod.Serialized)] Dictionary<string, string> channel, string guildId);

            [Headers("User-Agent: testPractice")]
            [Patch("/channels/{channelId}")]
            Task<Channel> ModifyChannel([Header("Authorization")] string token, string channelId, [Body(BodySerializationMethod.Serialized)] Dictionary<string, string> channel);

            [Headers("User-Agent: testPractice")]
            [Delete("/channels/{channelId}")]
            Task<HttpResponseMessage> DeleteChannel([Header("Authorization")] string token, string channelId);

        }
        public interface IDiscordImage
        {
            [Get("/icons/{guildId}/{imageIdHash}.png")]
            Task<HttpContent> GetGuildImage([Header("Authorization")] string token, string guildId, string imageIdHash);
        }

        private string access_token = string.Empty;
        private string bot_token = new StreamReader(@"C:\Users\zvina\source\repos\PracticeDiscordApi2\Bot_Token.txt").ReadToEnd();
        private IDiscordApi api = RestService.For<IDiscordApi>("https://discord.com/api");
        private List<Guild> guilds;
        private List<Channel> channels;
        private List<Channel> categories;
        private User currentUser;
        private Guild currentGuild;
        private Channel currentChannel;
        private int count = 0;

        private async void Enter_Clicked(object sender, EventArgs e)
        {
            if (Settings1.Default.Access_token != string.Empty)
            {
                access_token = Settings1.Default.Access_token;
            }
            else
            {
                string UrlTarget = @"https://discord.com/oauth2/authorize?client_id=1253598814854447205&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A5000%2Fcallback&scope=identify+guilds+gdm.join+rpc.notifications.read+rpc.video.read+rpc.screenshare.write+messages.read+role_connections.write+openid+applications.commands.permissions.update+email+guilds.join+rpc.voice.read+rpc.video.write+rpc.activities.write+rpc+rpc.voice.write+rpc.screenshare.read+connections+guilds.members.read";
#if WINDOWS
                Process.Start(new ProcessStartInfo
                {
                    FileName = UrlTarget,
                    UseShellExecute = true,
                });
#endif
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:5000/");
                listener.Start();
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                string code = request.Url.Query.Substring(6);
                Token token = await api.GetToken(new Dictionary<string, string>
                {
                    { "client_id", "1253598814854447205" },
                    { "client_secret", "Qu1thtAf-kIQdKLRzYC82NeZC25ENibe" },
                    { "grant_type", "authorization_code"},
                    { "code", code},
                    { "redirect_uri", "http://localhost:5000/callback"},
                });
                access_token = token.access_token;
                Settings1.Default.Access_token = access_token;
                Settings1.Default.Save();
            }
            currentUser = await api.GetCurrentUser($"Bearer {access_token}");
            WelcomeLabel.Text = $"Добро пожаловать: {currentUser.username}";
            afterEnter.IsVisible = true;
            guilds = await api.GetCurrentUserGuilds($"Bearer {access_token}");
            foreach (Guild guild in guilds)
            {
                guildPicker.Items.Add(guild.name);
            }
            //guildPicker.ItemsSource = guilds;
            /*            foreach (Guild guild in guilds) 
                        {
                            guildPicker.Items.Add(guild.ToString());
                            guildPicker.ItemsSource = guilds;
                        }*/
        }

        private void channel_sort(List<Channel> channelsTemp)
        {
            channels = new List<Channel>();
            categories = new List<Channel>();
            channelPicker.Items.Clear();
            channelParentPicker.Items.Clear();
            categories.Add(new Channel { name = " " });
            channelParentPicker.Items.Add(" ");
            //channels = channelsTemp.ToList();
            foreach (Channel c in channelsTemp)
                if (c.type != Channel_Types.GUILD_CATEGORY && c.parent_id == null)
                    channels.Add(c);
            foreach (Channel c in channelsTemp)
            {
                if (c.type == Channel_Types.GUILD_CATEGORY)
                {
                    channels.Add(c);
                    foreach (Channel c1 in channelsTemp)
                    {
                        if (c1.parent_id == c.id)
                        {
                            channels.Add(c1);
                        }
                    }
                }
            }
            bool flag = false;
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].type == Channel_Types.GUILD_CATEGORY)
                {
                    categories.Add(channels[i]);
                    channelParentPicker.Items.Add(channels[i].name);
                    channelPicker.Items.Add(channels[i].name);
                    flag = true;
                }
                else
                {
                    if (flag)
                    {
                        if (i + 1 < channels.Count)
                        {
                            if (channels[i + 1].type == Channel_Types.GUILD_CATEGORY)
                            {
                                channelPicker.Items.Add("└──" + channels[i].name);
                            }
                            else
                            {
                                channelPicker.Items.Add("├──" + channels[i].name);
                            }
                        }
                        else
                        {
                            channelPicker.Items.Add("└──" + channels[i].name);
                        }
                    }
                    else
                    {
                        channelPicker.Items.Add("───" + channels[i].name);
                    }
                }
            }
            StatusChannel.Text = null;
        }

        private async void guildPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guildPicker.SelectedIndex == -1 || guildPicker.SelectedItem is null) { guildImage.Source = null; guildImage.Background = null; guildText.Text = null; return; }
            guildText.Text = guildPicker.SelectedItem.ToString();
            if ((guilds[guildPicker.SelectedIndex]).icon != null)
            {
                guildImage.Source = $"https://cdn.discordapp.com/icons/{(guilds[guildPicker.SelectedIndex]).id}/{(guilds[guildPicker.SelectedIndex]).icon}.png?Authorization=Bearer ybteGurenNk18nyldYLRdnRBS0Pzh1";
                guildImage.Background = null;
            }
            else
            {
                guildImage.Source = null;
                guildImage.Background = Colors.DimGray;
            }
            currentGuild = guilds[guildPicker.SelectedIndex];

            try
            {
                channel_sort(await api.GetChannels($"Bot {bot_token}", currentGuild.id));
            }
            catch (Refit.ApiException ex)
            {
                StatusChannel.Text = "Данный бот не присутсвует на выбранном сервере";
            }


                /*IDiscordImage imageApi = RestService.For<IDiscordImage>("https://cdn.discordapp.com");
                HttpContent httpContent = await imageApi.GetGuildImage(access_token, (guildPicker.SelectedItem as Guild).id, (guildPicker.SelectedItem as Guild).icon);
                using (Stream stream = await httpContent.ReadAsStreamAsync())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        guildImage.Source = ImageSource.FromStream(() => memoryStream);
                    }
                }*/
            }

        private async void CreateGuildButton_Clicked(object sender, EventArgs e)
        {
            Guild newGuild = await api.CreateGuild($"Bot {bot_token}", new Dictionary<string, string> {
                { "name", guildText.Text }
            });
            HttpResponseMessage response = await api.InviteCurrentUser($"Bot {bot_token}", new Dictionary<string, string> { { "access_token", access_token } }, newGuild.id, currentUser.id);
            currentGuild = newGuild;
            //Enter_Clicked(sender, e);
            //guildPicker.Items.Clear();
            guilds.Add(newGuild);
            guildPicker.Items.Add(newGuild.name);

            //guildPicker.ItemsSource = guilds;
            //guildPicker.ItemsSource = guilds;
            //guildPicker.Items.Add(newGuild);
            guildPicker.SelectedIndex = guildPicker.Items.Count;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                StatusGuild.Text = "Сервер создан";
            }
            else
            {
                StatusGuild.Text = "Ошибка: " + response.StatusCode;
            }
        }

        private async void ModifyGuildButton_Clicked(object sender, EventArgs e)
        {
            if (currentGuild != null)
            {
                Guild result = await api.ModifyGuild($"Bot {bot_token}", currentGuild.id, new Dictionary<string, string>{
                    { "name", guildText.Text }
                });
                int index = guilds.IndexOf(currentGuild);
                guilds[index] = result;
                guildPicker.Items[index] = result.name;
                currentGuild = result;
                StatusGuild.Text = "Сервер изменён";
            }
            else
            {
                StatusGuild.Text = "Ошибка: выберите сервер";
            }
        }
        private async void DeleteGuildButton_Clicked(object sender, EventArgs e)
        {
            if (currentGuild != null)
            {
                HttpResponseMessage response = await api.DeleteGuild($"Bot {bot_token}", currentGuild.id);
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    guildPicker.Items.Remove(currentGuild.name);
                    guilds.Remove(currentGuild);
                    currentGuild = null;
                    guildPicker.SelectedIndex = -1;
                    StatusGuild.Text = "Сервер удалён";
                }
                else
                {
                    StatusGuild.Text = "Ошибка: " + response.StatusCode;
                }
            }
            else
            {
                StatusGuild.Text = "Ошибка: выберите сервер";
            }
        }

        private void channelPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (channelPicker.SelectedIndex != -1)
            {
                currentChannel = channels[channelPicker.SelectedIndex];
                channelText.Text = currentChannel.name;
                channelParentPicker.SelectedIndex = categories.FindIndex(p => p.id == currentChannel.parent_id);
                if (currentChannel.type == Channel_Types.GUILD_CATEGORY)
                {
                    categoryType.IsChecked = true;
                    channelType.IsChecked = false;
                    voiceType.IsChecked = false;
                    textType.IsChecked = false;
                }
                else
                {
                    categoryType.IsChecked = false;
                    channelType.IsChecked = true;
                    if (currentChannel.type == Channel_Types.GUILD_TEXT)
                    {
                        textType.IsChecked = true;
                        voiceType.IsChecked = false;
                    }
                    else
                    {
                        textType.IsChecked = false;
                        voiceType.IsChecked = true;
                    }
                }
            }
        }

        private void ChannelOrCatCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if ((sender == channelType) && (sender as RadioButton).IsChecked)
            {
                radioButtonsChannelType.IsVisible = true;
                channelParentPicker.IsVisible = true;
            }
            else
            {
                radioButtonsChannelType.IsVisible = false;
                channelParentPicker.IsVisible = false;
            }
        }

        private async void CreateChannelButton_Clicked(object sender, EventArgs e)
        {
            Channel newChannel;
            if ((categoryType.IsChecked || (channelType.IsChecked && (textType.IsChecked || voiceType.IsChecked))))
            {
                if (channelParentPicker.SelectedIndex != -1 || channelParentPicker.SelectedItem == "")
                {
                    newChannel = await api.CreateChannel($"Bot {bot_token}", new Dictionary<string, string>
                    {
                        { "name", channelText.Text },
                        { "type", ((int)(categoryType.IsChecked ? Channel_Types.GUILD_CATEGORY : voiceType.IsChecked ? Channel_Types.GUILD_VOICE : Channel_Types.GUILD_TEXT)).ToString() },
                        { "parent_id", categories[channelParentPicker.SelectedIndex].id }
                    }, currentGuild.id);
                }
                else
                {
                    newChannel = await api.CreateChannel($"Bot {bot_token}", new Dictionary<string, string>
                    {
                        { "name", channelText.Text },
                        { "type", ((int)(categoryType.IsChecked ? Channel_Types.GUILD_CATEGORY : voiceType.IsChecked ? Channel_Types.GUILD_VOICE : Channel_Types.GUILD_TEXT)).ToString() }
                    }, currentGuild.id);
                }
                channels.Add(newChannel);
                channel_sort(channels);
                StatusChannel.Text = "Канал успешно создан";
                currentChannel = newChannel;
                channelPicker.SelectedIndex = channels.IndexOf(newChannel);
            }
            else
                StatusChannel.Text = "Выберите тип канала";

        }
        private async void ModifyChannelButton_Clicked(object sender, EventArgs e)
        {
            if (currentChannel != null)
            {
                if ((categoryType.IsChecked ? Channel_Types.GUILD_CATEGORY : voiceType.IsChecked ? Channel_Types.GUILD_VOICE : Channel_Types.GUILD_TEXT) != currentChannel.type)
                {
                    StatusChannel.Text = "Ошибка: нельзя менять категорию канала";
                }
                else
                {
                    Channel result = await api.ModifyChannel($"Bot {bot_token}", currentChannel.id, new Dictionary<string, string>
                    {
                        { "name", channelText.Text },
                        { "parent_id", categories[channelParentPicker.SelectedIndex].id }
                    });
                    int index = channels.IndexOf(currentChannel);
                    channels[index] = result;
                    channel_sort(channels);
                    channelPicker.SelectedIndex = channels.IndexOf(result);
                    StatusChannel.Text = "Канал изменён";
                }
            }
            else
            {
                StatusChannel.Text = "Ошибка: выберите канал";
            }
        }
        private async void DeleteChannelButton_Clicked(object sender, EventArgs e)
        {
            if (currentChannel != null)
            {
                HttpResponseMessage response = await api.DeleteChannel($"Bot {bot_token}", currentChannel.id);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    channels.Remove(currentChannel);
                    currentChannel = null;
                    channel_sort(channels);
                    voiceType.IsChecked = false;
                    textType.IsChecked = false;
                    channelType.IsChecked = false;
                    categoryType.IsChecked = false;
                    channelText.Text = null;
                    StatusChannel.Text = "Канал успешно удалён";
                }
                else
                {
                    StatusChannel.Text = "Ошибка: " + response.StatusCode;
                }
            }
            else
            {
                StatusChannel.Text = "Ошибка: выберите канал";
            }
        }
    }

}
