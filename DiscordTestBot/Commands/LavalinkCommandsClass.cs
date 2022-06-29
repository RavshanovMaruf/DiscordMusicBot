using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using System.Linq;
using System;
using System.Text;
using System.Collections.Generic;

namespace DiscordTestBot.Commands
{
    public class LavalinkCommandsClass : BaseCommandModule
    {
        Queue<LavalinkTrack> queueOfTracks = new Queue<LavalinkTrack>();
        
        [Command("join")]
        [Description("Type !join VoiceChannelName to let bot enter voice channel")]
        public async Task Join(CommandContext ctx, DiscordChannel channel)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(channel).ConfigureAwait(false);
            await ctx.RespondAsync($"Joined {channel.Name}!");
        }

        [Command("leave")]
        [Description("Type !leave VoiceChannelName to let bot leave voice channel")]
        public async Task Leave(CommandContext ctx, DiscordChannel channel)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"Left {channel.Name}!");
        }
        
        [Command("play")]
        [Description("Type !play SongName to let bot play track")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();
            if (conn.CurrentState.CurrentTrack == null)
            {
                await conn.PlayAsync(track).ConfigureAwait(false);
                await ctx.RespondAsync($"Now playing {track.Title}!");
            }
            else
            {
                queueOfTracks.Enqueue(track);
                await ctx.RespondAsync($"{track.Title} added to queue!");
            }
            conn.PlaybackFinished += LavalinkPlaybackFinished;
        }
        private async Task LavalinkPlaybackFinished(LavalinkGuildConnection connection, TrackFinishEventArgs e)
        {
            var track = queueOfTracks.Dequeue();
            await connection.PlayAsync(track).ConfigureAwait(false);
        }

        [Command("skip")]
        [Description("Type !skip to skip cuurent track")]
        public async Task Skip(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync($"nothing to skip!");
            }
            if (conn.CurrentState.CurrentTrack != null)
            {
                var track = queueOfTracks.Dequeue();
                if(track == null)
                {
                    await ctx.RespondAsync($"there is no tracks in queue left!");
                    await conn.StopAsync();
                }
                else
                    await conn.PlayAsync(track);
            }
        }

        [Command("pause")]
        [Description("Type !pause to pause track that bot is playing")]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync().ConfigureAwait(false);
        }

        [Command("resume")]
        [Description("Resumes current track")]
        public async Task Resume(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.ResumeAsync().ConfigureAwait(false);
        }
        /*
        [Command("play")]
        [Description("Type !play SongName to let bot play track")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();
            
            
            await conn.PlayAsync(track).ConfigureAwait(false);

            await ctx.RespondAsync($"Now playing {track.Title}!");
        }
        
        private LavalinkNodeConnection Lavalink { get; set; }
        private LavalinkGuildConnection LavalinkVoice { get; set; }
        private DiscordChannel ContextChannel { get; set; }

        [Command, Description("Connects to Lavalink")]
        public async Task ConnectAsync(CommandContext ctx, string hostname, int port, string password)
        {
            if (this.Lavalink != null)
                return;

            var lava = ctx.Client.GetLavalink();
            if (lava == null)
            {
                await ctx.RespondAsync("Lavalink is not enabled.").ConfigureAwait(false);
                return;
            }

            this.Lavalink = await lava.ConnectAsync(new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint(hostname, port),
                SocketEndpoint = new ConnectionEndpoint(hostname, port),
                Password = password
            }).ConfigureAwait(false);
            this.Lavalink.Disconnected += this.Lavalink_Disconnected;
            await ctx.RespondAsync("Connected to lavalink node.").ConfigureAwait(false);
        }

        private Task Lavalink_Disconnected(LavalinkNodeConnection ll, NodeDisconnectedEventArgs e)
        {
            if (e.IsCleanClose)
            {
                this.Lavalink = null;
                this.LavalinkVoice = null;
            }
            return Task.CompletedTask;
        }

        [Command, Description("Disconnects from Lavalink")]
        public async Task DisconnectAsync(CommandContext ctx)
        {
            if (this.Lavalink == null)
                return;

            var lava = ctx.Client.GetLavalink();
            if (lava == null)
            {
                await ctx.RespondAsync("Lavalink is not enabled.").ConfigureAwait(false);
                return;
            }

            await this.Lavalink.StopAsync().ConfigureAwait(false);
            this.Lavalink = null;
            await ctx.RespondAsync("Disconnected from Lavalink node.").ConfigureAwait(false);
        }
        [Command("join")]
        [Description("Joins a voice channel.")]
        public async Task JoinAsync(CommandContext ctx, DiscordChannel chn = null)
        {
            if (this.Lavalink == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.").ConfigureAwait(false);
                return;
            }

            var vc = chn ?? ctx.Member.VoiceState.Channel;
            if (vc == null)
            {
                await ctx.RespondAsync("You are not in a voice channel or you did not specify a voice channel.").ConfigureAwait(false);
                return;
            }

            this.LavalinkVoice = await this.Lavalink.ConnectAsync(vc).ConfigureAwait(false);
            this.LavalinkVoice.PlaybackFinished += this.LavalinkVoice_PlaybackFinished;
            this.LavalinkVoice.DiscordWebSocketClosed += (s, e) => ctx.RespondAsync("discord websocket close event");
            await ctx.RespondAsync("Connected.").ConfigureAwait(false);
        }

        private async Task LavalinkVoice_PlaybackFinished(LavalinkGuildConnection ll, TrackFinishEventArgs e)
        {
            if (this.ContextChannel == null)
                return;

            await this.ContextChannel.SendMessageAsync($"Playback of {Formatter.Bold(Formatter.Sanitize(e.Track.Title))} by {Formatter.Bold(Formatter.Sanitize(e.Track.Author))} finished.").ConfigureAwait(false);
            this.ContextChannel = null;
        }
        [Command("leave")]
        [Description("Leaves a voice channel.")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.DisconnectAsync().ConfigureAwait(false);
            this.LavalinkVoice = null;
            await ctx.RespondAsync("Disconnected.").ConfigureAwait(false);
        }
        [Command("play")]
        [Description("Queues track for playback.")]
        public async Task PlaySoundCloudAsync(CommandContext ctx, string search)
        {
            if (this.Lavalink == null)
                return;

            var result = await this.Lavalink.Rest.GetTracksAsync(search, LavalinkSearchType.SoundCloud).ConfigureAwait(false);
            var track = result.Tracks.First();
            await this.LavalinkVoice.PlayAsync(track).ConfigureAwait(false);

            await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
        }
        [Command("play")]
        [Description("Queues tracks for playback.")]
        public async Task PlayPartialAsync(CommandContext ctx, TimeSpan start, TimeSpan stop, [RemainingText] Uri uri)
        {
            if (this.LavalinkVoice == null)
                return;

            var trackLoad = await this.Lavalink.Rest.GetTracksAsync(uri).ConfigureAwait(false);
            var track = trackLoad.Tracks.First();
            await this.LavalinkVoice.PlayPartialAsync(track, start, stop).ConfigureAwait(false);

            await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))}.").ConfigureAwait(false);
        }
        [Command("pause")]
        [Description("Pauses playback.")]
        public async Task PauseAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.PauseAsync().ConfigureAwait(false);
            await ctx.RespondAsync("Paused.").ConfigureAwait(false);
        }
        [Command("resume")]
        [Description("Resumes playback.")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.ResumeAsync().ConfigureAwait(false);
            await ctx.RespondAsync("Resumed.").ConfigureAwait(false);
        }
        [Command("stop")]
        [Description("Stops playback.")]
        public async Task StopAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.StopAsync().ConfigureAwait(false);
            await ctx.RespondAsync("Stopped.").ConfigureAwait(false);
        }

        [Command, Description("Seeks in the current track.")]
        public async Task SeekAsync(CommandContext ctx, TimeSpan position)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.SeekAsync(position).ConfigureAwait(false);
            await ctx.RespondAsync($"Seeking to {position}.").ConfigureAwait(false);
        }

        [Command, Description("Changes playback volume.")]
        public async Task VolumeAsync(CommandContext ctx, int volume)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.SetVolumeAsync(volume).ConfigureAwait(false);
            await ctx.RespondAsync($"Volume set to {volume}%.").ConfigureAwait(false);
        }

        [Command, Description("Shows what's being currently played."), Aliases("np")]
        public async Task NowPlayingAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            var state = this.LavalinkVoice.CurrentState;
            var track = state.CurrentTrack;
            await ctx.RespondAsync($"Now playing: {Formatter.Bold(Formatter.Sanitize(track.Title))} by {Formatter.Bold(Formatter.Sanitize(track.Author))} [{state.PlaybackPosition}/{track.Length}].").ConfigureAwait(false);
        }

        [Command, Description("Sets or resets equalizer settings."), Aliases("eq")]
        public async Task EqualizerAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.ResetEqualizerAsync().ConfigureAwait(false);
            await ctx.RespondAsync("All equalizer bands were reset.").ConfigureAwait(false);
        }

        [Command]
        public async Task EqualizerAsync(CommandContext ctx, int band, float gain)
        {
            if (this.LavalinkVoice == null)
                return;

            await this.LavalinkVoice.AdjustEqualizerAsync(new LavalinkBandAdjustment(band, gain)).ConfigureAwait(false);
            await ctx.RespondAsync($"Band {band} adjusted by {gain}").ConfigureAwait(false);
        }

        [Command, Description("Displays Lavalink statistics.")]
        public async Task StatsAsync(CommandContext ctx)
        {
            if (this.LavalinkVoice == null)
                return;

            var stats = this.Lavalink.Statistics;
            var sb = new StringBuilder();
            sb.Append("Lavalink resources usage statistics: ```")
                .Append("Uptime:                    ").Append(stats.Uptime).AppendLine()
                .Append("Players:                   ").AppendFormat("{0} active / {1} total", stats.ActivePlayers, stats.TotalPlayers).AppendLine()
                .Append("CPU Cores:                 ").Append(stats.CpuCoreCount).AppendLine()
                .Append("CPU Usage:                 ").AppendFormat("{0:#,##0.0%} lavalink / {1:#,##0.0%} system", stats.CpuLavalinkLoad, stats.CpuSystemLoad).AppendLine()
                .Append("RAM Usage:                 ").AppendFormat("{0} allocated / {1} used / {2} free / {3} reservable", SizeToString(stats.RamAllocated), SizeToString(stats.RamUsed), SizeToString(stats.RamFree), SizeToString(stats.RamReservable)).AppendLine()
                .Append("Audio frames (per minute): ").AppendFormat("{0:#,##0} sent / {1:#,##0} nulled / {2:#,##0} deficit", stats.AverageSentFramesPerMinute, stats.AverageNulledFramesPerMinute, stats.AverageDeficitFramesPerMinute).AppendLine()
                .Append("```");
            await ctx.RespondAsync(sb.ToString()).ConfigureAwait(false);
        }

        private static readonly string[] Units = new[] { "", "ki", "Mi", "Gi" };
        private static string SizeToString(long l)
        {
            double d = l;
            var u = 0;
            while (d >= 900 && u < Units.Length - 2)
            {
                u++;
                d /= 1024;
            }

            return $"{d:#,##0.00} {Units[u]}B";
        }*/
    
    }
}
