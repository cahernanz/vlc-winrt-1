/*****************************************************************************
 * vlc_vout.h: common video definitions
 *****************************************************************************
 * Copyright (C) 1999 - 2008 VLC authors and VideoLAN
 * $Id: 7c17cc144f0aaf56635628a15d69a7523c9108c9 $
 *
 * Authors: Vincent Seguin <seguin@via.ecp.fr>
 *          Samuel Hocevar <sam@via.ecp.fr>
 *          Olivier Aubert <oaubert 47 videolan d07 org>
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2.1 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston MA 02110-1301, USA.
 *****************************************************************************/

#ifndef VLC_VOUT_H_
#define VLC_VOUT_H_ 1

#include <vlc_es.h>
#include <vlc_picture.h>
#include <vlc_subpicture.h>

/**
 * \defgroup output Output
 * \defgroup video_output Video output
 * \ingroup output
 * Video rendering, output and window management
 *
 * This module describes the programming interface for video output threads.
 * It includes functions allowing to open a new thread, send pictures to a
 * thread, and destroy a previously opened video output thread.
 * @{
 * \file
 * Video output thread interface
 */

/**
 * Vout configuration
 */
typedef struct {
    vout_thread_t        *vout;
    vlc_object_t         *input;
    bool                 change_fmt;
    const video_format_t *fmt;
    unsigned             dpb_size;
} vout_configuration_t;

/**
 * Video output thread private structure
 */
typedef struct vout_thread_sys_t vout_thread_sys_t;

/**
 * Video output thread descriptor
 *
 * Any independent video output device, such as an X11 window or a GGI device,
 * is represented by a video output thread, and described using the following
 * structure.
 */
struct vout_thread_t {
    VLC_COMMON_MEMBERS

    /* Private vout_thread data */
    vout_thread_sys_t *p;
};

/* Alignment flags */
#define VOUT_ALIGN_LEFT         0x0001
#define VOUT_ALIGN_RIGHT        0x0002
#define VOUT_ALIGN_HMASK        0x0003
#define VOUT_ALIGN_TOP          0x0004
#define VOUT_ALIGN_BOTTOM       0x0008
#define VOUT_ALIGN_VMASK        0x000C

/**
 * Viewpoints
 */
struct vlc_viewpoint_t {
    float yaw;   /* yaw in degrees */
    float pitch; /* pitch in degrees */
    float roll;  /* roll in degrees */
    float fov;   /* field of view in degrees */
    float zoom;  /* zoom factor, [-1.f, 1.f] range, default to 0.f */
};

static inline void vlc_viewpoint_init( vlc_viewpoint_t *p_vp )
{
    p_vp->yaw = p_vp->pitch = p_vp->roll = p_vp->zoom = 0.0f;
    p_vp->fov = DEFAULT_FIELD_OF_VIEW_DEGREES;
}

/*****************************************************************************
 * Prototypes
 *****************************************************************************/

/**
 * Returns a suitable vout or release the given one.
 *
 * If cfg->fmt is non NULL and valid, a vout will be returned, reusing cfg->vout
 * is possible, otherwise it returns NULL.
 * If cfg->vout is not used, it will be closed and released.
 *
 * You can release the returned value either by vout_Request or vout_Close()
 * followed by a vlc_object_release() or shorter vout_CloseAndRelease()
 *
 * \param object a vlc object
 * \param cfg the video configuration requested.
 * \return a vout
 */
VLC_API vout_thread_t * vout_Request( vlc_object_t *object, const vout_configuration_t *cfg );
#define vout_Request(a,b) vout_Request(VLC_OBJECT(a),b)

/**
 * This function will close a vout created by vout_Request.
 * The associated vout module is closed.
 * Note: It is not released yet, you'll have to call vlc_object_release()
 * or use the convenient vout_CloseAndRelease().
 *
 * \param p_vout the vout to close
 */
VLC_API void vout_Close( vout_thread_t *p_vout );

/**
 * This function will close a vout created by vout_Create
 * and then release it.
 *
 * \param p_vout the vout to close and release
 */
static inline void vout_CloseAndRelease( vout_thread_t *p_vout )
{
    vout_Close( p_vout );
    vlc_object_release( p_vout );
}

/**
 * This function will handle a snapshot request.
 *
 * pp_image, pp_picture and p_fmt can be NULL otherwise they will be
 * set with returned value in case of success.
 *
 * pp_image will hold an encoded picture in psz_format format.
 *
 * i_timeout specifies the time the function will wait for a snapshot to be
 * available.
 *
 */
VLC_API int vout_GetSnapshot( vout_thread_t *p_vout,
                              block_t **pp_image, picture_t **pp_picture,
                              video_format_t *p_fmt,
                              const char *psz_format, mtime_t i_timeout );

VLC_API void vout_ChangeAspectRatio( vout_thread_t *p_vout,
                                     unsigned int i_num, unsigned int i_den );

/* */
VLC_API picture_t * vout_GetPicture( vout_thread_t * );
VLC_API void vout_PutPicture( vout_thread_t *, picture_t * );

/* */
VLC_API void vout_PutSubpicture( vout_thread_t *, subpicture_t * );
VLC_API int vout_RegisterSubpictureChannel( vout_thread_t * );
VLC_API void vout_FlushSubpictureChannel( vout_thread_t *, int );

VLC_API void vout_EnableFilter( vout_thread_t *, const char *,bool , bool  );

/**@}*/

#endif /* _VLC_VIDEO_H */
