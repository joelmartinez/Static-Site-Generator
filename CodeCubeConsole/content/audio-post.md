Title: Dynamic Audio Posts
Date: 2019-07-27  
Published: true  

Trying out something new ... adding some dynamic audio generation
capabilities to my blog posts, using [tone.js](https://tonejs.github.io/). 
Turn up your speakers and let's see if this works:

<blockquote>
    <button id="dapButton">Play Sound</button>
</blockquote>

<script>
document.postcontext = (function()
{
    var context = {};
    context.synth = new Tone.Synth().toMaster();

    var dapButton = document.getElementById("dapButton");
    var lowNote=true;
    console.log("setting up post context");
    dapButton.onclick=function() {
        var note = "E2";
        if (!lowNote) note="F2";
        console.log("playing " + note);
        context.synth.triggerAttackRelease(note, "8n");
        lowNote = !lowNote;
    }

    return context;
})();
</script>

Go ahead, press the button a few times, if you can recognize it,
[send me a tweet!](https://twitter.com/joelmartinez)