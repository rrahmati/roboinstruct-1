###############################################################################
#                                                                             #
# Copyright (C) 2010 Edward d'Auvergne                                        #
#                                                                             #
# This file is part of the program relax.                                     #
#                                                                             #
# relax is free software; you can redistribute it and/or modify               #
# it under the terms of the GNU General Public License as published by        #
# the Free Software Foundation; either version 2 of the License, or           #
# (at your option) any later version.                                         #
#                                                                             #
# relax is distributed in the hope that it will be useful;                    #
# but WITHOUT ANY WARRANTY; without even the implied warranty of              #
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the               #
# GNU General Public License for more details.                                #
#                                                                             #
# You should have received a copy of the GNU General Public License           #
# along with relax; if not, write to the Free Software                        #
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA   #
#                                                                             #
###############################################################################

# Module docstring.
"""Module for transforming between different coordinate systems."""

# Python module imports.
from math import acos, atan2, cos, sin
from numpy import array, float64, zeros
from numpy.linalg import norm


def cartesian_to_spherical(vector):
    """Convert the Cartesian vector [x, y, z] to spherical coordinates [r, theta, phi].

    The parameter r is the radial distance, theta is the polar angle, and phi is the azimuth.


    @param vector:  The Cartesian vector [x, y, z].
    @type vector:   numpy rank-1, 3D array
    @return:        The spherical coordinate vector [r, theta, phi].
    @rtype:         numpy rank-1, 3D array
    """

    # The radial distance.
    r = norm(vector)

    # Unit vector.
    unit = vector / r

    # The polar angle.
    theta = acos(unit[2])

    # The azimuth.
    phi = atan2(unit[1], unit[0])

    # Return the spherical coordinate vector.
    return r, theta, phi


def spherical_to_cartesian(spherical_vect):
    """Convert the spherical coordinate vector [r, theta, phi] to the Cartesian vector [x, y, z].

    The parameter r is the radial distance, theta is the polar angle, and phi is the azimuth.


    @param spherical_vect:  The spherical coordinate vector [r, theta, phi].
    @type spherical_vect:   3D array or list
    @param cart_vect:       The Cartesian vector [x, y, z].
    @type cart_vect:        3D array or list
    """

    # Trig alias.
    sin_theta = sin(spherical_vect[1])

    # The vector.
    x = spherical_vect[0] * cos(spherical_vect[2]) * sin_theta
    y = spherical_vect[0] * sin(spherical_vect[2]) * sin_theta
    z = spherical_vect[0] * cos(spherical_vect[1])
    return x, y, z

# print cartesian_to_spherical(array([1.4,1.3,0.2]))