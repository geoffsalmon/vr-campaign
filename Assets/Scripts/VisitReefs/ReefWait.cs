using UnityEngine;
using System.Collections;


// example of IReefVisitor that controls when to leave a reef
public class ReefWait : MonoBehaviour, IReefVisitor {
	public float WaitTime = 6;

	private ReefStage stage;

	private bool waitingAtReef = false;
	private float finishTime;

	public void visitingReef(ReefStage stage) {
		this.stage = stage;
	}

	void Update () {
		if (!waitingAtReef) {
			if (stage != null) {
				if (stage.VisitReefs.State == VisitReefs.VisitReefsState.AtReef) {
					waitingAtReef = true;
					finishTime = Time.time + WaitTime;
				}
			}
		} else if (Time.time >= finishTime) {
			// call finishStage to stop this component from delaying the transit to the next reef
			stage.finishStage();
		}
	}
}
