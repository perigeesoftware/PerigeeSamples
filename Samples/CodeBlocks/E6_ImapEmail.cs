using MailKit;
using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Watchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.CodeBlocks
{

    /*
        ---- IMAP Email Watcher ----
        -- https://docs.perigee.software/core-modules/event-sources/watchers/imap

        IMAP Email Watcher demonstrates how to set up and manage an IMAP email watcher to automatically respond to incoming messages.

        --== Learning Objective #1: Initial Setup of IMAP Email Watcher ==--
        * Learn how to initialize an IMAP email watcher to monitor an inbox.
        * Understand the configuration parameters such as email address, password, IMAP and SMTP hosts, and ports.
        * Observe the setup of an IMAP watcher for email monitoring.

        --== Learning Objective #2: Handling Incoming Emails ==--
        * Learn how to process incoming emails that are not yet answered.
        * Understand how to generate and send a reply to the incoming email.
        * Observe the use of MailKit for replying to emails and marking them as answered.

        --== Learning Objective #3: Error Handling and Logging ==--
        * Understand the importance of error handling when processing emails.
        * Learn how to log errors and mark emails with appropriate flags and labels in case of exceptions.
        * Observe the detailed logging of email processing, including errors and successful completions.

        --== Learning Objective #4: Practical Application of Email Watching ==--
        * Learn why using an IMAP email watcher can enhance email handling and automation in real-world applications.
        * Understand the practical benefits of using an email watcher for automating email responses.
        * Observe how the IMAP email watcher ensures efficient and reliable email monitoring and processing.

        --> To run this sample:
        E6_ImapEmail.run();
    */

    public static class E6_ImapEmail
    {
        public static void run()
        {
            PerigeeApplication.ApplicationNoInit("MailDemo", (c) => {

                //Add an IMAP watcher to reply to messages from an inbox
                //You can also use the SASL authentication provided by MailKit. 
                //  Perigee has a built in SASL for Gmail as well: MailWatcher.SASL_GoogleAPIS()
                c.AddIMAPWatcher("MailWatch",
                    "email@address.com", "MailBot", 
                    "unsecurepassword",
                    "IMAPhost", 993,
                    "SMTPhost", 587, 
                    (ct, l, mail) =>
                    {
                        try
                        {
                            if (!mail.IsAnswered)
                            {
                                //Generate a response
                                var reply = mail.Reply(true, (b) => {
                                    b.TextBody = "We received your message and are working on the request!";

                                }, includeReplyText: true);
                                mail.SendMessage(reply);


                                //Mark it on success
                                mail.AddFlags(MessageFlags.Answered | MessageFlags.Seen);
                                mail.AddLabels("success");
                            }
                        }
                        catch (Exception ex)
                        {
                            l.LogError(ex, "Uncaught exception in mail processor");
                            try
                            {
                                mail.AddFlags(MessageFlags.Answered | MessageFlags.Seen);
                                mail.AddLabels("error");
                            }
                            catch (Exception) { }
                        }
                    });
            });
        }
    }
}
