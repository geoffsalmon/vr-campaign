using UnityEngine;
using System.Collections;

public class CubicBezier {
	public Vector3 p0 { get; private set; }
	public Vector3 p1 { get; private set; }
	public Vector3 p2 { get; private set; }
	public Vector3 p3 { get; private set; }

	public CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
		this.p0 = p0;
		this.p1 = p1;
		this.p2 = p2;
		this.p3 = p3;
	}

	public Vector3 position(float t) {
		float u = 1 - t;
		return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
	}

	public Vector3 derivative(float t) {
		float u = 1 - t;
		return 3 * u * u * (p1 - p0) + 6 * u * t * (p2 - p1) + 3 * t * t * (p3 - p2);
	}

	public Vector3 second_derivative(float t) {
		float u = 1 - t;
		return 6 * u * (p2 - 2*p1 + p0) + 6 * t * (p3 - 2*p2 + p1);
	}

	/*public void curve(float t, out Vector3 position, out Vector3 derivative) {
		float u = 1 - t;
		derivative = 3 * u * u * (p1 - p0) + 6 * u * t * (p2 - p1) + 3 * t * t * (p3 - p2);
	}
	\mathbf{B}(t)=(1-t)^3\mathbf{P}_0+3(1-t)^2t\mathbf{P}_1+3(1-t)t^2\mathbf{P}_2+t^3\mathbf{P}_3 \mbox{ , } 0 \le t \le 1.
		For some choices of P1 and P2 the curve may intersect itself, or contain a cusp.

			Any series of any 4 distinct points can be converted to a cubic Bézier curve that goes through all 4 points in order. Given the starting and ending point of some cubic Bézier curve, and the points along the curve corresponding to t = 1/3 and t = 2/3, the control points for the original Bézier curve can be recovered.[5]

			The derivative of the cubic Bézier curve with respect to t is

				\mathbf{B}'(t) = 3(1-t)^2(\mathbf{P}_1 - \mathbf{P}_0) + 6(1-t)t(\mathbf{P}_2 - \mathbf{P}_1) + 3t^2(\mathbf{P}_3 - \mathbf{P}_2) \,.
The second derivative of the Bézier curve with respect to t is

\mathbf{B}''(t) = 6(1-t)(\mathbf{P}_2 - 2 \mathbf{P}_1 + \mathbf{P}_0) +  6t(\mathbf{P}_3 - 2 \mathbf{P}_2 + \mathbf{P}_1) \,.
*/

}
