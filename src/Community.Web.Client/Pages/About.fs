namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.Ui

/// About page — feature-owned view. A static page; owns no state and needs no
/// message case (per message-organization: "Static pages do not need a message
/// case"). Built on Radzen text + card primitives.
module About =

    /// The community blurb (same length discipline as the FAQ answers).
    let aboutBlurb =
        "42WASD is a gaming community hub: browse the games we play, watch "
        + "**live server capacity**, and follow tournaments as registration opens."

    /// FAQ answers (kept short here so the accordion tree reads cleanly).
    let faqJoin =
        "Create an account from the Account page — any username works in this "
        + "demo build. You'll appear in the Members roster right away."
    let faqServers =
        "The Servers page lists every game server with live player capacity. "
        + "Bars turn amber near full and red when the server is locked."
    let faqTournaments =
        "Open tournaments accept registration from their card on the Tournaments "
        + "page. Closing registration locks new entries; reopening re-opens the gate."

    let view () =
        // Audit #7: wall-of-prose → a section card with an icon+title heading
        // row, and the tech list as quiet chips (the genre-badge primitive).
        RadzenUI.vStackGap "var(--gap-section)" (concat {
            RadzenUI.pageHeadingCrumb "About"
                (Some "What this community hub is built on.")
                [ ("Home", Some "/"); ("About", None) ]
            // RadzenFieldset with legend (42-switches #19) + RadzenMarkdown
            // body (42-switches #21) replace the hand-rolled card+heading+
            // divider composite; collapsible for compact reading.
            // FAQ accordion (42-audit #37): the standard community-page Q&A.
            // Answers live in short named lets — the tree stays readable.
            RadzenUI.accordion (concat {
                RadzenUI.accordionItem "How do I join the community?"
                    (RadzenUI.text RadzenUI.body1 faqJoin)
                RadzenUI.accordionItem "What are the live servers?"
                    (RadzenUI.text RadzenUI.body1 faqServers)
                RadzenUI.accordionItem "How do tournaments work?"
                    (RadzenUI.text RadzenUI.body1 faqTournaments)
            })
            // Social/contact links row (42-audit #38): RadzenLink icons.
            RadzenUI.hStackGap "1rem" (concat {
                RadzenUI.link "https://github.com" (RadzenUI.icon "code")
                RadzenUI.link "mailto:community@42wasd.example" (RadzenUI.icon "mail")
            })
            RadzenUI.fieldset "The community" true (concat {
                RadzenUI.markdown aboutBlurb
                RadzenUI.hStackGap "0.5rem" (concat {
                    RadzenUI.chip "Bolero" RadzenUI.primaryBadge
                    RadzenUI.chip "Elmish" RadzenUI.primaryBadge
                    RadzenUI.chip "F#" RadzenUI.primaryBadge
                    RadzenUI.chip "Radzen Blazor" RadzenUI.primaryBadge
                })
            })
        })