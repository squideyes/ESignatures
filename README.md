**SquidEyes.ESignatures** is a "fluent" C# client for the <a href="https://esignatures.io/" target="_blank">eSigntures.io</a> contract-signing web-service, open-sourced on <a href="https://github.com/squideyes/SquidEyes.ESignatures" target="_blank">GitHub</a> (under an **MIT license**; see License.md for details) and available as a NuGet. 

The client is meant to be paired with a custom web-hook-processor like the included **WebHookProcessor**.  A small bit of C# code (**ContractSender**) will also be needed to "kickoff" the contract-signing process and then you'd typically monitor the resulting Azure Service Bus messages with a program like **MessageProcessor**.


To use the demos you'll need to signup for a demo <a href="https://esignatures.io/" target="_blank">eSignatures.io</a> account.  Once the account is created, you'll then need to do the following:

1. Click on the "**API & Automation**" menu item (on the uper right-hand corner of the  eSignatures.com site) and then take note of **Your Secret Token**.  You'll need it to run the ContractSender demo.
2. Go to the https://esignatures.io/contract_templates web page and then import the **MarketingAgreement.esiot** file (located in the SquidEyes.ESignatures solution folder).
3. Take note of the **TemplateId** (a smallish light-gray GUID, labeled "**ID**," and found in the upper left-hand corner of the web-page).  You'll need it to run the ContractSender demo.
4. Install <a href="https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio" target="_blank">Azurite</a>, if you haven't done so already. Azurite is a local emulator for Azure Blob Storage.
5. Install <a href="https://ngrok.com/download" target="_blank">NGROK</a>; a command-line utility that facilitates the receipt of web-hooks messages by code running on your local computer.
6. Run "**ngrok http http://localhost:7159**" in a command window and then take note of the **Forwarding** address (you can ignore the local address, after the arrow).
7. Run **WebHookProcessor**.  As a side-effect, an "**webhook-received**" queue  will be automatically created in Azurite, if it doesn't exist already. 
8. Configure the following settings in the ContractSender **appsettings.json** file:
- **TemplateId**: TemplateId from Step #3, above
- **BaseUri**: NGROK Forwarding address from Step #6, above
- **CcPdfTos**: Zero or more semicolon-separated email addresses
- **VendorReplyTo**: An optional "reply-to" email address
- **VendorEmail** Vendor's email address
- **VendorMobile**: Vendor's mobile phone number
- **VendorLogoUri**: URI to vendor's 128x128 pixel logo
9. Run **ContractSender** with the following command line arguments
- **--AuthToken=**{**Your Secret Token**, from Step #1, above}
- **--PartnerEmail=**{Partner's  email address}
- **--PartnerMobile=**{Partner's mobile phone number}

As an alternative, you may choose to keep your AuthToken in a UserSecrets file, with the JSON formatted as follows: {"AuthToken": "**Your Secret Token**"}

You can use the same email and phone number for both the Vendor and Partner, to simplify the testing process.  In either case, only use an email or mobile phone number that you own since the Partner and Vendor signers will need to respond to an email and then to fill in a number on the resulting form with a 6-digit code sent via SMS.

The fluent **ContractSender** has been designed to make kicking off the contract-signing process easy (see the ContractSender's Program.cs for for details):

```csharp
    var request = new ContractSender<Metadata>(authToken, contract)
        .WithSignerInfo(p.Signer, p.Handling, p.Address)
        .WithSignerInfo(v.Signer, v.Handling, v.Address)
        .WithPlaceholder("client-id", metadata.ClientId)
        .WithPlaceholder("doc-code", metadata.DocCode)
        .AsTest();

    (await request.SendAsync(cts.Token)).Switch(
        HandleAccepted, HandleRejected, HandleFailed, HandleCancelled);
```
NOTE #1: **WebHookProcessor** was written in C#, but inasmuch as the code takes no dependencies on the SquidEyes.ESignatures assembly, the functionality might be implemented in a variety of suitable languages (i.e. Node.js, Go, Python, etc.)

NOTE #2: The enclosed code has been developed for the author's own scenarios, and as such it exercises a less-than-complete fraction of the sSignatures.io functionality.  In particular, no provision has been made for both the creation and management of templates nor for embedded signing.  Moreover, the library is rather opinionated and bespoke (i.e. the code receives and processes a variety of web-hook messages but makes no provision for stateful message management).