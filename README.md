# Vidusaviya.shasra

*(Discontinued Project)*  

This project were to be a **free - open source online classroom web app**.   

As the 2020 lockdown began, school started online education but had to pay for online streaming services. Me and my friend [Harindu](https://github.com/hariwij) started this as a side project for our School Managment system Vidusaviya.

# Technologies 

 - Microsoft Blazor web assembly
 - WebRTC
 - Github Octokit
 - Google Firebase

# Design
*Free means Free. Not a single cent.* Thus, we **cannot have backend servers**.  
But how the content is delivered?  
Simple solution - **Github Octokit !**  

Yeah, Github have very powerful servers capable of delivering *small* data files for **free**. Using C# Blazor webassembly - C# that runs inside the browser, we designed this serverless online streaming web app.

For ones who want **more** realtime performance, they can simply switch to (Included) Google Firebase **Firestore**. 

# Progress made 

 - [x] Recording Video/Audio via Javascript Media API  
 - [x] Compressing, Encoding media and serving them to Web Assembly  
 - [x] Encrypting media and uploading to Github via octokit  
 - [ ] Peers getting Realtime webhook notification  
 - [x] Peers downloading media packets  (HTTP) 
 - [x] Decrypting and serving to Javascript 
 - [x] Javascript decoding media to Media Source API  
 - [x] Playing in the HTML5 Player  
 - [ ] Peers (Students) sharing their live (snapshot) images via WebRTC peer-to-peer network
 - [ ]  Account management server (Currently this uses Account Manager integrated to Vidusaviya project)

**Where did we stuck?**
We used WebM format to record and play media. We selected it for the maximum compatibility for all the devices. However, WebM is not suitable for realtime media streaming. **If you interest building your own media streaming solution**, grab this repo and use HLS on javascript side.  

**Contributions are highly appreciated**  
  
    
**Developers**
*[Hasindu Lanka](https://github.com/HasinduLanka)  
[Harindu Wijesinghe](https://github.com/hariwij)*   


# Extra Content - Octokit .NET Blazor
The original Octokit .NET cannot be used for C# Blazor WASM (Even the NuGet). Therefore, we ported it to use in the browser. Anyone having trouble with using octokit in blazor, Just clone this repo and get the "Octokit" project.

