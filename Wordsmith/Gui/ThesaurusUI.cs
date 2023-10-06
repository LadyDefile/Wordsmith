﻿using ImGuiNET;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Wordsmith.Helpers;
using Wordsmith;
using Dalamud.Interface.Utility;
namespace Wordsmith.Gui;

internal sealed class ThesaurusUI : Window, IDisposable
{
    private string _query = "";

    private MerriamWebsterAPI SearchHelper;

    internal static string GetWindowName() => $"{Wordsmith.AppName} - Thesaurus";
    /// <summary>
    /// Instantiates a new ThesaurusUI object.
    /// </summary>
    public ThesaurusUI() : base(GetWindowName())
    {
        SearchHelper = new MerriamWebsterAPI();

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = ImGuiHelpers.ScaledVector2(375, 330),
            MaximumSize = ImGuiHelpers.ScaledVector2(9999, 9999)
        };

        Flags |= ImGuiWindowFlags.NoScrollbar;
        Flags |= ImGuiWindowFlags.NoScrollWithMouse;
        //Flags |= ImGuiWindowFlags.MenuBar;
    }

    #region Drawing
    /// <summary>
    /// The Draw entry point for Dalamud.Interface.Windowing
    /// </summary>
    public override void Draw()
    {
        // Create a child element for the word search.
        if ( ImGui.BeginChild( "Word Search Window" ) )
        {
            DrawSearchBar();
            if ( ImGui.BeginChild( "SearchResultWindow" ) )
            {
                DrawSearchErrors();
                for ( int i = 0, c = SearchHelper.History.Count; i < c; )
                {
                    WordSearchResult result = SearchHelper.History[i];
                    if ( DrawSearchResult( result ) )
                        i++;
                    else
                    {
                        c--;
                        SearchHelper.DeleteResult(result);
                    }
                }

                // End the child element.
                ImGui.EndChild();
            }
            ImGui.EndChild();
        }
    }

    /// <summary>
    /// Draws the search bar on the UI.
    /// </summary>
    private void DrawSearchBar()
    {
        float btnWidth = 100*ImGuiHelpers.GlobalScale;
        ImGui.SetNextItemWidth( ImGui.GetWindowContentRegionMax().X - btnWidth - ImGui.GetStyle().FramePadding.X * 2 );

        if ( ImGui.InputTextWithHint( "###ThesaurusSearchBar", "Search...", ref _query, 128, ImGuiInputTextFlags.EnterReturnsTrue ) )
        {
            SearchHelper.SearchThesaurus( this._query );
            this._query = "";
        }

        ImGui.SameLine();
        if ( ImGui.Button( "Search##ThesaurusSearchButton", new( btnWidth, 0 ) ) )
        {
            SearchHelper.SearchThesaurus( this._query );
            this._query = "";
        }

        ImGui.Separator();
    }

    /// <summary>
    /// Draws the last search's data to the UI.
    /// </summary>
    private void DrawSearchErrors()
    {
        if ( SearchHelper.State == ApiState.Failed )
        {
            ImGui.TextColored( new Vector4( 255, 0, 0, 255 ), $"Search failed. Try again or use a different word." );
            ImGui.Separator();
        }
        else if ( SearchHelper.State == ApiState.Searching )
        {
            ImGui.Text( "Searching..." );
            ImGui.Separator();
        }
    }

    /// <summary>
    /// Draws one search result item to the UI.
    /// </summary>
    /// <param name="result">The search result to be drawn</param>
    /// <returns><see langword="true"/> if visible; otherwise <see langword="false"/>.</returns>
    private bool DrawSearchResult(WordSearchResult result)
    {
        if (result != null)
        {
            // Default to visible.
            bool vis = true;
            if (ImGui.CollapsingHeader($"{result.Query.Trim().CaplitalizeFirst()}##{result.ID}", ref vis, ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                foreach (ThesaurusEntry entry in result?.Entries ?? Array.Empty<ThesaurusEntry>())
                    DrawEntry(entry);
                ImGui.Unindent();
            }

            // return the visibility state.
            return vis;
        }
        return false;
    }

    /// <summary>
    /// Draws a search result's entry data. One search result can have multiple data entries.
    /// </summary>
    /// <param name="entry">The data to draw.</param>
    private void DrawEntry(ThesaurusEntry entry)
    {
        if ( ImGui.CollapsingHeader($"{entry.Type.Trim().CaplitalizeFirst()} - {entry.Definition.Replace("{it}", "").Replace("{/it}", "")}##{entry.ID}"))
        {
            ImGui.Indent();

            if (entry.Synonyms.Count > 0)
            {
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.TextWrapped("Synonyms");
                ImGui.TextWrapped(entry.SynonymString);
            }

            if (entry.Related.Count > 0)
            {
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.TextWrapped("Related Words");
                ImGui.TextWrapped(entry.RelatedString);
            }

            if (entry.NearAntonyms.Count > 0)
            {
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.TextWrapped("Near Antonyms");
                ImGui.TextWrapped(entry.NearAntonymString);
            }

            if (entry.Antonyms.Count > 0)
            {
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.TextWrapped("Antonyms");
                ImGui.TextWrapped(entry.AntonymString);
            }
            ImGui.Unindent();
        }
    }
    #endregion

    /// <summary>
    ///  Disposes of the SearchHelper child.
    /// </summary>
    public void Dispose() => this.SearchHelper.Dispose();
}
