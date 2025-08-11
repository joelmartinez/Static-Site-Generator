Title: How You Ship Matters
Date: 2025-08-11
Published: true
Prev: /2025/8/team-series-specialization/
Hero: /posts/design-vs-ship-hero.png

When you look at how teams ship software, two extremes pop into view.  
On one side you have the conservative design process ... roadmaps, stakeholder sign-off, formal requirements, wireframes, the works. Nothing moves until the picture is complete.  
On the other hand, you've got the classic startup “move fast and break things” ... ship early, ship often, learn by doing, fix it live.

Both approaches have their place. Early-stage teams lean toward shipping because the goal is to find product/market fit before the money or attention runs out. Large, regulated orgs lean toward upfront design because stability, compliance, and predictability matter more than speed.  

NASA, especially in the era of the Space Shuttle program, leaned heavily toward the conservative side. Of course there, the cost of failure was measured in human lives and billions of dollars, so they used waterfall-like processes with meticulous upfront design. Entire subsystems were engineered with redundant backups before any construction began. This worked for rockets and spaceflight, but the same approach applied to an early-stage startup would grind it to a halt before the first release.

Agile was supposed to bridge these worlds... design *just enough*, then course-correct quickly with real feedback. The idea was to avoid locking into bad assumptions while still protecting the team from chaos.  
In practice, Agile often gets watered down into a mechanical set of ceremonies: standups without shared purpose, sprints that don’t iterate, retros with no follow-through. The *principles* fade, but the calendar invites stay.

## The churn factor

Since [this series](/2025/7/team-series/) is focused on how teams grow, I wanted to focus here on one thing that often nudges teams away from the “just ship it” end of the spectrum ... **requirements churn**.

In the early phase, churn can be healthy. It’s the constant rethinking that helps a small group discover the product together. PMs, designers, and ICs ([if you even have such diverse roles yet](/2025/8/team-series-specialization/)) are all close to the problem. They can pivot quickly because everyone’s in the same conversations, sometimes literally in the same room.

As the team grows, accountability and ownership shift and ICs go from deciding “what to do next” to being told what’s next. That’s natural in a larger org, it’s part of the tendency to shift to specialization as a team grows, but it _changes the cost of change_. But now every tweak has to be communicated across roles: PM to design to engineering, maybe across squads or time zones. Every change means re-aligning multiple people. When churn is high, this constant re-alignment creates uncertainty, stress, and, eventually, burnout ... pain.

Leadership’s natural reaction is to shield the team. That usually means more process: tighter requirements, more design sign-off, feature freeze periods. It reduces the whiplash, but it also slows cadence ... and pretty soon, leadership is asking why features take so long to ship now. And the answer is simple: you traded some speed for stability.

## Making the trade-offs deliberately

The reality here is that you need to make conscious trade-offs based on where you are ... not only in your team/company's lifecycle, but also individual projects. It's ok to change how you work situationally.

- **Design the minimum surface area** you need to avoid expensive rewrites.  
- Keep iteration loops real: learn, adapt, revalidate. Don’t just re-skin bad assumptions.  
- Use lightweight design rituals:
  - Constrained design reviews.
  - Pre-mortems to call out failure points early.
  - Tiny prototypes instead of polished decks.

Your balance will change as the team changes. What works at five people can hurt at fifty. Re-evaluate often. The best process is the one that matches your current moment, and the worst mistake is clinging to a process just because it worked once.